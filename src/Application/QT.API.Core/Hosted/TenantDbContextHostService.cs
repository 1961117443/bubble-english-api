using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QT.Common.Cache;
using QT.Common.Configuration;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Dtos.OAuth;
using QT.Common.Extension;
using QT.Common.Security;
using QT.Extras.DatabaseAccessor.SqlSugar;
using QT.FriendlyException;
using QT.RemoteRequest.Extensions;
using QT.TaskScheduler;
using QT.TaskScheduler.Entitys.Entity;
using QT.TaskScheduler.Entitys.Model;
using Serilog;
using SqlSugar;
using CSRedis;
using System.Reflection;
using QT.TaskScheduler.Entitys.Enum;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Systems.Interfaces.System;
using QT.Systems;
using Mapster;
using QT.Common.Contracts;
using Minio.DataModel;

namespace QT.API.Core;

/// <summary>
/// 租户后台服务，获取租户数据库
/// </summary>
public class TenantDbContextHostService : BackgroundService
{
    private const string redisMessageTopic = "erptenant:dbcontext:change";
    private readonly IMemoryCache _memoryCache;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="memoryCache"></param>
    public TenantDbContextHostService(IMemoryCache memoryCache, IServiceProvider serviceProvider)
    {
        _memoryCache = memoryCache;
        _serviceProvider = serviceProvider;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (KeyVariable.MultiTenancy)
        {
            try
            {

                // 获取所有正常的租户
                var interFace = string.Format("{0}{1}", KeyVariable.MultiTenancyDBInterFace, "Normal/List");
                var response = await interFace.SetRetryPolicy(3,5000).GetAsAsync<List<TenantListInterFaceOutput>>();

#if DEBUG
                if (response.IsAny())
                {
                    foreach (var item in response)
                    {
                        if (item.linkList.IsAny())
                        {
                            foreach (var item1 in item.linkList)
                            {
                                if (item1.host == "192.168.1.20")
                                {
                                    item1.host = "219.151.28.155";
                                    item1.port = "33397";
                                }

                                if (item1.connectionStr.IsNotEmptyOrNull())
                                {
                                    item1.connectionStr = item1.connectionStr.Replace("192.168.1.20", "219.151.28.155").Replace("3396", "33397");
                                }
                            }
                        }
                    }
                }
#endif

                //var xitem = response.FirstOrDefault(x => x.enCode == "1001");
                //if (xitem!=null && xitem.linkList.IsAny())
                //{
                //    //xitem.linkList[0]
                //    //xitem.linkList.ForEach(x =>
                //    //{
                //    //    x.connectionStr=x.connectionStr.Replace("172.16.1.100", "117.21.203.8")
                //    //    .Replace("3306", "33100");
                //    //});
                //}

                var list = InitTenantDbList(response);
                if (list.IsAny())
                {
#if !DEBUG
                    await StartTimerJob(list);
#endif
                }

                // 清除在线用户
                foreach (var item in list)
                {
                    TenantScoped.Create(item.enCode, (factory, scope) =>
                    {
                        var _cacheManager = scope.ServiceProvider.GetService<ICacheManager>();
                        if (_cacheManager != null)
                        {
                            _cacheManager.Del(string.Format("{0}{1}", CommonConst.CACHEKEYONLINEUSER, item.enCode));
                        }
                    });
                }

                // 启动租户定时任务
                foreach (var item in list)
                {
                    // 忽略日志数据库
                    if (item is TenantInterFaceOutput tenant && tenant.linkList.IsAny() && tenant.linkList.Any(x=>x.configType == 2))
                    {
                        continue;
                    }
                    CreateTenantTask(item.enCode, item.enCode);
                }

            }
            catch (Exception ex)
            {
                Log.Error($"初始化获取租户失败！{ex.Message}");
            }

            // 如果使用redis缓存的话，使用发布订阅更新租户数据库信息
            if (KeyVariable.CacheType == CacheType.RedisCache)
            {
                //// 初始化RedisHelper
                _ = _serviceProvider.GetService<RedisCache>();
                Action<CSRedisClient.SubscribeMessageEventArgs> action = msg =>
                {
                    Console.WriteLine("租户数据库发生变动：" + msg.Body);

                    if (!string.IsNullOrEmpty(msg.Body))
                    {
                        try
                        {
                            //var currentList =  _memoryCache.Get<ITenantDbInfo>("tenant:dbcontext:list");
                            var response = msg.Body.ToObject<List<TenantListInterFaceOutput>>();
                            var list = InitTenantDbList(response);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, redisMessageTopic);
                        }
                    }
                };
                RedisHelper.Subscribe((redisMessageTopic, action));
            }


        }
        else
        {
            CreateTenantTask(Guid.NewGuid().ToString(), "");
        }
    }

    #region

    /// <summary>
    /// 初始化租户数据库
    /// </summary>
    /// <returns></returns>
    private List<ITenantDbInfo> InitTenantDbList(List<TenantListInterFaceOutput> response)
    {
        List<ITenantDbInfo> list = new List<ITenantDbInfo>();
        if (response.IsAny())
        {
            List<ConnectionConfig> configs = new List<ConnectionConfig>();
            foreach (var item in response)
            {
                Dictionary<string, TenantListInterFaceOutput> dbList = new()
                {
                    { item.enCode, item }
                };
                // 判断是否启用独立日志库
                var logLinkList = item.linkList.Where(x => x.configType == 2).ToList();
                if (logLinkList.IsAny())
                {
                    item.linkList = item.linkList.Except(logLinkList).ToList();
                    var logdb = new TenantListInterFaceOutput();
                    item.Adapt(logdb);
                    logdb.enCode = $"{item.enCode}_log";
                    logdb.linkList = logLinkList.Take(1).ToList();
                    dbList.Add(logdb.enCode, logdb);
                }
                foreach (var kv in dbList)
                {
                    var config = SqlSugarHelper.CreateConnectionConfig(kv.Key, kv.Value);
                    using (var db = new SqlSugarClient(config))
                    {
                        var ado = db.CopyNew().Context.Ado;
                        if (ado.Connection.ConnectionString.IndexOf("Connection Timeout") == -1)
                        {
                            var ext = (ado.Connection.ConnectionString.EndsWith(";") ? "" : ";") + "Connection Timeout=3;";
                            ado.Connection.ConnectionString = $"{ado.Connection.ConnectionString}{ext}";
                        }
                        if (ado.IsValidConnectionNoClose())
                        {
                            configs.Add(config);
                            list.Add(kv.Value);
                        }
                    }
                }
                
            }
            // 租户数据库的配置信息
            _memoryCache.Set("tenant:dbcontext:configs", configs);
        }
        // 租户数据库数据，包含开始和结束时间
        _memoryCache.Set("tenant:dbcontext:list", list);
        return list;
    }

    /// <summary>
    /// 启动租户定时任务
    /// </summary>
    /// <returns></returns>
    private async Task StartTimerJob(List<ITenantDbInfo> list)
    {
        // 启动各个租户的定时任务
        foreach (var item in list)
        {
            // 忽略日志数据库
            if (item is TenantInterFaceOutput tenant && tenant.linkList.IsAny() && tenant.linkList.Any(x => x.configType == 2))
            {
                continue;
            }
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    //var tenant = scope.ServiceProvider.GetRequiredService<IScopedTenant>();
                    //tenant.Login(item.enCode);

                    var cacheType = TenantCacheFactory.GetOrCreate(KeyVariable.CacheType, item.enCode);
                    var _cache = scope.ServiceProvider.GetService(cacheType) as ICache;
                    var cacheManager = CacheManager.CreateInstance(_cache);

                    // 清空定时任务缓存
                    await cacheManager.DelAsync(CommonConst.CACHEKEYTIMERJOB);

                    var rep = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<TimeTaskEntity>>();
                    rep.Context.ChangeDatabase(item.enCode);
                    //var service = scope.ServiceProvider.GetRequiredService<ITimeTaskService>();

                    var jobs = await rep.Where(x => x.DeleteMark == null && x.EnabledMark == 1).ToListAsync();

                    if (jobs.IsAny())
                    {
                        foreach (var job in jobs)
                        {
                            // 防止不用的租户用同一个数据库
                            if (SpareTime.GetWorker(job.Id) == null)
                            {
                                this.AddTimerJob(job, cacheManager);

                                Log.Information(job.ToJsonString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"租户【{item.enCode}】启动定时任务失败！");
                }
            }
        }
    }

    /// <summary>
    /// 添加定时任务
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async void AddTimerJob(TimeTaskEntity input, ICacheManager cacheManager)
    {
        Action<SpareTimer, long>? action = null;
        ContentModel? comtentModel = input.ExecuteContent.ToObject<ContentModel>();
        input.ExecuteCycleJson = comtentModel.cron;
        switch (input.ExecuteType)
        {
            case "3":
                // 查询符合条件的任务方法
                TaskMethodInfo? taskMethod = (await GetTaskMethods(cacheManager))?.FirstOrDefault(m => m.id == comtentModel.localHostTaskId);
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
    private async Task<string> PerformJob(TimeTaskEntity entity)
    {
        try
        {
            var model = entity.ExecuteContent.ToObject<ContentModel>();
            var parameters = model.parameter.ToDictionary(key => key.field, value => value.value.IsNotEmptyOrNull() ? value.value : value.defaultValue);

            var actuatorType = TenantCacheFactory.GetOrCreate(typeof(TenantDataInterfaceActuator<>), model.TenantId);
            using (var scope = _serviceProvider.CreateScope())
            {
                var actuator = scope.ServiceProvider.GetService(actuatorType) as IDataInterfaceActuator;
                await actuator.GetResponseByType(model.interfaceId, 3, model.TenantId, null, parameters);
            }
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
    private async Task<List<TaskMethodInfo>> GetTaskMethods(ICacheManager _cacheManager)
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
    /// 创建租户定时任务，每分钟检查一次
    /// </summary>
    private void CreateTenantTask(string workerName,string tenantCode)
    {
        // 每个租户开启一个定时任务，执行间隔1分钟，根据不同的数据库，执行不同的任务
        SpareTime.Do(60 * 1000, async (st, c) =>
        {
            string tenantId = string.Empty;
            if (st.Description.IsNotEmptyOrNull())
            {
                tenantId = st.Description.Split("/")[0];
            }

            //Log.Information($"【{workerName}】【{tenantCode}】即将执行租户【{tenantId}】的定时任务...");
            // 判断租户是否有效
            await TenantScoped.Create(tenantId, async (scopeFactory, scope) =>
            {
                //await Task.Delay(1500);
                var services = scope.ServiceProvider.GetService<IEnumerable<IOneMinuteActuator>>();
                if (services.IsAny())
                {
                    foreach (var service in services)
                    {
                        await service.Execute();
                    }
                }
                //Log.Information($"【{workerName}】【{tenantCode}】执行完毕租户【{tenantId}】的定时任务...");
            });

        }, workerName, tenantCode + "/" + tenantCode+ "/TaskLog=0", true, executeType: SpareTimeExecuteTypes.Serial);

    }
    #endregion
}
