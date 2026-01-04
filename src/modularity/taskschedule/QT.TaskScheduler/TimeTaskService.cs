using System.Reflection;
using QT.Common.Configuration;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.LinqBuilder;
using QT.Systems.Interfaces.System;
using QT.TaskScheduler.Entitys.Dto.TaskScheduler;
using QT.TaskScheduler.Entitys.Entity;
using QT.TaskScheduler.Entitys.Enum;
using QT.TaskScheduler.Entitys.Model;
using QT.TaskScheduler.Interfaces.TaskScheduler;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Common.Core.Security;

namespace QT.TaskScheduler;

/// <summary>
/// 定时任务
/// </summary>
[ApiDescriptionSettings(Tag = "TaskScheduler", Name = "scheduletask", Order = 220)]
[Route("api/[controller]")]
public class TimeTaskService : ITimeTaskService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<TimeTaskEntity> _timeTaskRepository;
    private readonly IDataInterfaceService _dataInterfaceService;
    private readonly IUserManager _userManager;
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 初始化一个<see cref="TimeTaskService"/>类型的新实例.
    /// </summary>
    public TimeTaskService(
        ISqlSugarRepository<TimeTaskEntity> timeTaskRepository,
        IUserManager userManager,
        IDataInterfaceService dataInterfaceService,
        ICacheManager cacheManager)
    {
        _timeTaskRepository = timeTaskRepository;
        _userManager = userManager;
        _dataInterfaceService = dataInterfaceService;
        _cacheManager = cacheManager;
    }

    #region Get

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input">请求参数</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] PageInputBase input)
    {
        var queryWhere = LinqExpression.And<TimeTaskEntity>().And(x => x.DeleteMark == null);
        if (!string.IsNullOrEmpty(input.keyword))
            queryWhere = queryWhere.And(m => m.FullName.Contains(input.keyword) || m.EnCode.Contains(input.keyword));
        var list = await _timeTaskRepository.AsQueryable().Where(queryWhere).OrderBy(x => x.CreatorTime, OrderByType.Desc)
            .OrderByIF(!string.IsNullOrEmpty(input.keyword), t => t.LastModifyTime, OrderByType.Desc).ToPagedListAsync(input.currentPage, input.pageSize);
        var pageList = new SqlSugarPagedList<TimeTaskListOutput>()
        {
            list = list.list.Adapt<List<TimeTaskListOutput>>(),
            pagination = list.pagination
        };
        return PageResult<TimeTaskListOutput>.SqlSugarPageResult(pageList);
    }

    /// <summary>
    /// 列表（执行记录）.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <param name="id">任务Id.</param>
    /// <returns></returns>
    [HttpGet("{id}/TaskLog")]
    public async Task<dynamic> GetTaskLogList([FromQuery] TaskLogInput input, string id)
    {
        var whereLambda = LinqExpression.And<TimeTaskLogEntity>().And(x => x.TaskId == id);
        if (input.runResult.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(x => x.RunResult == input.runResult);
        if (input.endTime != null && input.startTime != null)
        {
            var start = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 00:00:00}", input.startTime?.TimeStampToDateTime()));
            var end = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 23:59:59}", input.endTime?.TimeStampToDateTime()));
            whereLambda = whereLambda.And(x => SqlFunc.Between(x.RunTime, start, end));
        }
        var list = await _timeTaskRepository.Context.UseLogDatabase().Queryable<TimeTaskLogEntity>().SplitTable().Where(whereLambda).OrderBy(x => x.RunTime, OrderByType.Desc).ToPagedListAsync(input.currentPage, input.pageSize);
        var pageList = new SqlSugarPagedList<TimeTaskTaskLogListOutput>()
        {
            list = list.list.Adapt<List<TimeTaskTaskLogListOutput>>(),
            pagination = list.pagination
        };
        return PageResult<TimeTaskTaskLogListOutput>.SqlSugarPageResult(pageList);
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("Info/{id}")]
    public async Task<dynamic> GetInfo_Api(string id)
    {
        return (await GetInfo(id)).Adapt<TimeTaskInfoOutput>();
    }

    /// <summary>
    /// 本地方法.
    /// </summary>
    /// <returns></returns>
    [HttpGet("TaskMethods")]
    public async Task<dynamic> GetTaskMethodSelector()
    {
        return await GetTaskMethods();
    }
    #endregion

    #region Post

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">实体对象</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] TimeTaskCrInput input)
    {
        if (await _timeTaskRepository.AnyAsync(x => (x.EnCode == input.enCode || x.FullName == input.fullName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var comtentModel = input.executeContent.ToObject<ContentModel>();
        comtentModel.TenantId = _userManager.TenantId;
        comtentModel.TenantDbName = _userManager.TenantDbName;
        var entity = input.Adapt<TimeTaskEntity>();
        entity.ExecuteContent = comtentModel.ToJsonString();
        entity.ExecuteCycleJson = comtentModel.cron;
        var result = await _timeTaskRepository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();
        _ = result ?? throw Oops.Oh(ErrorCode.COM1000);

        // 添加到任务调度里
        AddTimerJob(result);

        await SyncClusterSpareTime("timer_task_add",result);
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键值</param>
    /// <param name="input">实体对象</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] TimeTaskUpInput input)
    {
        if (await _timeTaskRepository.AnyAsync(x => x.Id != id && (x.EnCode == input.enCode || x.FullName == input.fullName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var entityOld = await GetInfo(id);

        // 先从调度器里取消
        SpareTime.Cancel(id);
        var entityNew = input.Adapt<TimeTaskEntity>();
        entityNew.RunCount = entityOld.RunCount;
        entityNew.EnabledMark = entityOld.EnabledMark;
        var comtentModel = input.executeContent.ToObject<ContentModel>();
        comtentModel.TenantId = _userManager.TenantId;
        comtentModel.TenantDbName = _userManager.TenantDbName;
        entityNew.ExecuteContent = comtentModel.ToJsonString();
        entityNew.ExecuteCycleJson = comtentModel.cron;
        var isOk = await _timeTaskRepository.Context.Updateable(entityNew).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);

        // 再添加到任务调度里
        if (entityNew.EnabledMark == 1)
        {
            AddTimerJob(entityNew);
        }

        await SyncClusterSpareTime("timer_task_update", entityNew);
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await GetInfo(id);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        var isOk = await _timeTaskRepository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
        // 从调度器里取消
        SpareTime.Cancel(entity.Id);

        await SyncClusterSpareTime("timer_task_del", entity);
    }

    /// <summary>
    /// 停止.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/Stop")]
    public async Task Stop(string id)
    {
        var isOk = await _timeTaskRepository.Context.Updateable<TimeTaskEntity>().SetColumns(it => new TimeTaskEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id.Equals(id)).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1003);
        SpareTime.Stop(id);

        await SyncClusterSpareTime("timer_task_stop", new TimeTaskEntity { Id = id});
    }

    /// <summary>
    /// 启动.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/Enable")]
    public async Task Enable(string id)
    {
        var entity = await GetInfo(id);
        var isOk = await _timeTaskRepository.Context.Updateable<TimeTaskEntity>().SetColumns(it => new TimeTaskEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id.Equals(id)).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1003);

        var comtentModel = entity.ExecuteContent.ToObject<ContentModel>();
        comtentModel.TenantId = _userManager.TenantId;
        comtentModel.TenantDbName = _userManager.TenantDbName;
        entity.ExecuteContent = comtentModel.ToJsonString();
        var timer = SpareTime.GetWorkers().ToList().Find(u => u.WorkerName == id);
        if (timer == null)
        {
            AddTimerJob(entity);
        }
        else
        {
            // 如果 StartNow 为 flase , 执行 AddTimerJob 并不会启动任务
            SpareTime.Start(entity.Id);
        }

        await SyncClusterSpareTime("timer_task_enable", entity);
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 启动自启动任务.
    /// </summary>
    [NonAction]
    public void StartTimerJob()
    {
        // 非多租户模式启动自启任务
        if (!KeyVariable.MultiTenancy)
        {
            _timeTaskRepository.ToList(x => x.DeleteMark == null && x.EnabledMark == 1).ForEach(AddTimerJob);
        }
    }
    #endregion

    #region PrivateMethod

    /// <summary>
    /// 详情.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [NonAction]
    private async Task<TimeTaskEntity> GetInfo(string id)
    {
        return await _timeTaskRepository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
    }

    /// <summary>
    /// 新增定时任务.
    /// </summary>
    /// <param name="input"></param>
    [NonAction]
    public async void AddTimerJob(TimeTaskEntity input)
    {
        Action<SpareTimer, long>? action = null;
        ContentModel? comtentModel = input.ExecuteContent.ToObject<ContentModel>();
        input.ExecuteCycleJson = comtentModel.cron;
        switch (input.ExecuteType)
        {
            case "3":
                // 查询符合条件的任务方法
                TaskMethodInfo? taskMethod = GetTaskMethods()?.Result.FirstOrDefault(m => m.id == comtentModel.localHostTaskId);
                if (taskMethod == null) break;

                // 创建任务对象
                object? typeInstance = Activator.CreateInstance(taskMethod.DeclaringType);

                // 创建委托
                action = (Action<SpareTimer, long>)Delegate.CreateDelegate(typeof(Action<SpareTimer, long>), typeInstance, taskMethod.MethodName);
                break;
            default:
                action = async (timer, count) =>
                {
                    var msg = await PerformJob(input);
                };
                break;
        }

        if (action == null) return;
        SpareTime.Do(comtentModel.cron, action, input.Id, comtentModel.TenantId + "/" + comtentModel.TenantDbName, true, executeType: SpareTimeExecuteTypes.Parallel);
    }

    /// <summary>
    /// 根据类型执行任务.
    /// </summary>
    /// <param name="entity">任务实体.</param>
    /// <returns></returns>
    public async Task<string> PerformJob(TimeTaskEntity entity)
    {
        try
        {
            var model = entity.ExecuteContent.ToObject<ContentModel>();
            var parameters = model.parameter.ToDictionary(key => key.field, value => value.value.IsNotEmptyOrNull() ? value.value : value.defaultValue);
            await _dataInterfaceService.GetResponseByType(model.interfaceId, 3, model.TenantId, null, parameters);
            return string.Empty;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    /// <summary>
    /// 获取所有本地任务.
    /// </summary>
    /// <returns></returns>
    private async Task<List<TaskMethodInfo>> GetTaskMethods()
    {
        var taskMethods = await _cacheManager.GetAsync<List<TaskMethodInfo>>(CommonConst.CACHEKEYTIMERJOB);
        if (taskMethods != null) return taskMethods;
        // 获取所有本地任务方法，必须有spareTimeAttribute特性
        taskMethods = App.EffectiveTypes
            .Where(u => u.IsClass && !u.IsInterface && !u.IsAbstract && typeof(ISpareTimeWorker).IsAssignableFrom(u))
            .SelectMany(u => u.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.IsDefined(typeof(SpareTimeAttribute), false) &&
                   m.GetParameters().Length == 2 &&
                   m.GetParameters()[0].ParameterType == typeof(SpareTimer) &&
                   m.GetParameters()[1].ParameterType == typeof(long) && m.ReturnType == typeof(void))
            .Select(m =>
            {
                // 默认获取第一条任务特性
                var spareTimeAttribute = m.GetCustomAttribute<SpareTimeAttribute>();
                return new TaskMethodInfo
                {
                    id = $"{m.DeclaringType.Name}/{m.Name}",
                    fullName = spareTimeAttribute.WorkerName,
                    RequestUrl = $"{m.DeclaringType.Name}/{m.Name}",
                    cron = spareTimeAttribute.CronExpression,
                    DoOnce = spareTimeAttribute.DoOnce,
                    ExecuteType = spareTimeAttribute.ExecuteType,
                    Interval = (int)spareTimeAttribute.Interval / 1000,
                    StartNow = spareTimeAttribute.StartNow,
                    RequestType = RequestTypeEnum.Run,
                    Remark = spareTimeAttribute.Description,
                    TimerType = string.IsNullOrEmpty(spareTimeAttribute.CronExpression) ? SpareTimeTypes.Interval : SpareTimeTypes.Cron,
                    MethodName = m.Name,
                    DeclaringType = m.DeclaringType
                };
            })).ToList();
        await _cacheManager.SetAsync(CommonConst.CACHEKEYTIMERJOB, taskMethods);
        return taskMethods;
    }

    /// <summary>
    /// 同步集群中的定时任务
    /// </summary>
    /// <returns></returns>
    private async Task SyncClusterSpareTime(string topic, TimeTaskEntity entity)
    {
        if (KeyVariable.CacheType == Common.Cache.CacheType.RedisCache)
        {
            TimeTaskSyncModel taskSyncModel = new TimeTaskSyncModel
            {
                TenantId = _userManager.TenantId,
                WorkerId = SnowflakeIdHelper.CurrentWorkerId(),
                Data = entity,
            };
            await RedisHelper.PublishAsync(topic, taskSyncModel.ToJsonString());
        }
    }
    #endregion
}