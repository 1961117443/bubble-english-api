using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extend.Entitys.Dto.TeamLog;
using QT.Extend.Entitys.Dto.WoekLog;
using QT.Extend.Entitys.Dto.WorkLog;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;
using SqlSugar.Extensions;
using Yitter.IdGenerator;

namespace QT.Extend;

/// <summary>
    /// 工作日志
    /// </summary>
[ApiDescriptionSettings(Tag = "Extend", Name = "WorkLog", Order = 600)]
[Route("api/extend/[controller]")]
public class WorkLogService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<WorkLogEntity> _workLogRepository;
    private readonly IUsersService _usersService;
    private readonly IUserManager _userManager;
    private readonly ITenant _db;

    public WorkLogService(ISqlSugarRepository<WorkLogEntity> workLogRepository, IUsersService usersService, IUserManager userManager, ISqlSugarClient context)
    {
        _workLogRepository = workLogRepository;
        _usersService = usersService;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    #region Get

    /// <summary>
    /// 列表(个人日志列表)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<PageResult<WorkLogListOutput>> GetList([FromQuery] WorkLogListPageInput input)
    {
        DateTime? startDate = null,endDate =null;
        DateTime? startTime = null, endTime = null;
        if (input.startTime.HasValue)
        {
            startTime = input.startTime.Value.TimeStampToDateTime();
        }
        if (input.endTime.HasValue)
        {
            endTime = input.endTime.Value.TimeStampToDateTime().AddDays(1).AddSeconds(-1);
        }
        if (input.date.IsNotEmptyOrNull())
        {
            if (input.date.Length == 10)
            {
                if (DateTime.TryParse(input.date, out var dt))
                {
                    startDate = dt;
                    endDate = dt.AddDays(1).AddSeconds(-1);
                }
            }
            else if (int.TryParse(input.date,out var date))
            {
                if (DateTime.TryParse($"{input.date}-01-01", out var dt))
                {
                    startDate = dt;
                    endDate = dt.AddYears(1).AddSeconds(-1);
                }
            }
            else
            {
                if (DateTime.TryParse($"{input.date}-01", out var dt))
                {
                    startDate = dt;
                    endDate = dt.AddMonths(1).AddSeconds(-1);
                }
            }
        }
        var list = await _workLogRepository.AsQueryable().Where(x => x.CreatorUserId == _userManager.UserId && x.DeleteMark == null)
            .WhereIF(input.keyword.IsNotEmptyOrNull(), m => m.TodayContent.Contains(input.keyword))
            .WhereIF(startTime.HasValue, m => SqlFunc.GreaterThanOrEqual(m.CreatorTime, startTime))
            .WhereIF(endTime.HasValue, m => SqlFunc.LessThanOrEqual(m.CreatorTime, endTime))
            .WhereIF(startDate.HasValue, m => SqlFunc.GreaterThanOrEqual(m.CreatorTime, startDate))
            .WhereIF(endDate.HasValue, m => SqlFunc.LessThanOrEqual(m.CreatorTime, endDate))
            .OrderBy(x => x.CreatorTime, OrderByType.Desc)
            .OrderByIF(!string.IsNullOrEmpty(input.keyword), t => t.LastModifyTime, OrderByType.Desc)
            .Select(x => new WorkLogListOutput
            {
                sender = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == x.CreatorUserId).Select(ddd => ddd.RealName),
                creatorTime = x.CreatorTime,
                //creatorUserId = x.CreatorUserId
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);

        //var userIds = list.list.Select(x => x.creatorUserId).ToList();
        //if (userIds.IsAny())
        //{
        //    var users = await _repository.Context.Queryable<UserEntity>().Where(x => userIds.Contains(x.Id))
        //        .Select(x => new UserEntity
        //        {
        //            Id = x.Id,
        //            RealName = x.RealName,
        //            Account = x.Account,
        //            HeadIcon = x.HeadIcon,
        //        })
        //        .ToListAsync();

        //    foreach (var item in list.list)
        //    {
        //        item.managerInfo = users.Find(x => x.Id == item.creatorUserId)?.Adapt<ManagersInfo>() ?? new ManagersInfo(); ;
        //    }
        //}

        return PageResult<WorkLogListOutput>.SqlSugarPagedList(list);
    }

    /// <summary>
    /// 列表(我发出的)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Send")]
    public async Task<dynamic> GetSendList([FromQuery] WorkLogListPageInput input)
    {
        DateTime? startTime=null, endTime = null;
        if (input.startTime.HasValue)
        {
            startTime = input.startTime.Value.TimeStampToDateTime();
        }
        if (input.endTime.HasValue)
        {
            endTime = input.endTime.Value.TimeStampToDateTime().AddDays(1).AddSeconds(-1);
        }
        var list = await _workLogRepository.AsQueryable().Where(x => x.CreatorUserId == _userManager.UserId && x.DeleteMark == null)
            .WhereIF(input.keyword.IsNotEmptyOrNull(), m => m.Title.Contains(input.keyword) || m.Description.Contains(input.keyword))
            .WhereIF(input.type == "draft", m => m.EnabledMark == 0)
            .WhereIF(input.type == "sent", m => m.EnabledMark == 1)
            .WhereIF(startTime.HasValue,m=>SqlFunc.GreaterThanOrEqual(m.CreatorTime,startTime))
            .WhereIF(endTime.HasValue, m => SqlFunc.LessThanOrEqual(m.CreatorTime, endTime))
            .OrderBy(x => x.SortCode).OrderBy(x => x.CreatorTime, OrderByType.Desc)
            .OrderByIF(!string.IsNullOrEmpty(input.keyword), t => t.LastModifyTime, OrderByType.Desc).ToPagedListAsync(input.currentPage, input.pageSize);


        if (list.list.IsAny())
        {
            var workLogIdList = list.list.Select(x => x.Id).ToList();
            var users = await _workLogRepository.Context.Queryable<WorkLogShareEntity>().Where(x => workLogIdList.Contains(x.WorkLogId))
                .Select(x => new UserEntity
                {
                    Id = x.WorkLogId,
                    RealName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == x.ShareUserId).Select(ddd => ddd.RealName)
                })
                .ToListAsync();

            foreach (var item in list.list)
            {
                item.ToUserId = string.Join(",", users.Where(x => x.Id == item.Id).Select(x => x.RealName));
            }
        }


        var pageList = new SqlSugarPagedList<WorkLogListOutput>()
        {
            list = list.list.Adapt<List<WorkLogListOutput>>(),
            pagination = list.pagination
        };
        return PageResult<WorkLogListOutput>.SqlSugarPageResult(pageList);
    }

    /// <summary>
    /// 列表(我收到的)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Receive")]
    public async Task<dynamic> GetReceiveList([FromQuery] PageInputBase input)
    {
        var list = await _workLogRepository.Context.Queryable<WorkLogEntity, WorkLogShareEntity>(
            (a, b) => new JoinQueryInfos(JoinType.Left, a.Id == b.WorkLogId))
            .Where((a, b) => a.DeleteMark == null && b.ShareUserId == _userManager.UserId)
            .WhereIF(input.keyword.IsNotEmptyOrNull(), a => a.Title.Contains(input.keyword))
            .Select(a => new WorkLogListOutput()
            {
                id = a.Id,
                title = a.Title,
                question = a.Question,
                creatorTime = a.CreatorTime,
                todayContent = a.TodayContent,
                tomorrowContent = a.TomorrowContent,
                toUserId = a.ToUserId,
                sortCode = a.SortCode,
                lastModifyTime = a.LastModifyTime,
                sender = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == a.CreatorUserId).Select(ddd => ddd.RealName),
                attachJson = a.AttachJson,
                imageJson = a.ImageJson,
                category = a.Category
            }).MergeTable()
            .OrderBy(a => a.sortCode).OrderBy(a => a.creatorTime, OrderByType.Desc)
            .OrderByIF(!string.IsNullOrEmpty(input.keyword), t => t.lastModifyTime, OrderByType.Desc)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<WorkLogListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _workLogRepository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null)).Adapt<WorkLogInfoOutput>();
        output.userIds = output.toUserId;
        output.toUserId = await _usersService.GetUserName(output.toUserId);
        return output;
    }

    /// <summary>
    /// 按日期汇总日志数据
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    [HttpGet("Actions/LogSumByMon")]
    public async Task<List<WorkLogSumOutput>> LogSumByMon(string teamId)
    {
        var list = await _workLogRepository.AsQueryable()
            .Where(x=>x.CreatorUserId == _userManager.UserId)
            .Where(x => x.DeleteMark == null)
            .Select(x => new WorkLogSumOutput
            {
                id = x.CreatorTime.Value.ToString("yyyy-MM-dd"),
                parentId = x.CreatorTime.Value.ToString("yyyy-MM"),
                mon = x.CreatorTime.Value.ToString("yyyy-MM"),
                year = x.CreatorTime.Value.ToString("yyyy")
            })
            .MergeTable()
            .GroupBy(x => new { x.id ,x.parentId})
            .Select(x => new WorkLogSumOutput
            {
                id = x.id,
                parentId = x.parentId,
                count = SqlFunc.AggregateCount(x.id),
                mon = SqlFunc.AggregateMax(x.mon),
                year = SqlFunc.AggregateMax(x.year)
            })
            .OrderByDescending(x => x.id)
            .ToListAsync();

        if (list.IsAny())
        {
            var parents = list.GroupBy(x => new { x.mon ,x.year}).Select(x => new WorkLogSumOutput
            {
                id = x.Key.mon,
                parentId = x.Key.year,
                count = x.Sum(a => a.count)
            }).ToList();

            var yearSum = list.GroupBy(x => x.year).Select(x => new WorkLogSumOutput
            {
                id = x.Key,
                parentId = "",
                count = x.Sum(a => a.count)
            }).ToList();

            list.AddRange(parents);


            list.AddRange(yearSum);
        }
        
        return list.ToTree("");
    }

    /// <summary>
    /// 日志汇总查询
    /// </summary>
    /// <param name="input"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("Actions/Personal")]
    public async Task<PageResult<WorkLogPersonalOutput>> PersonalList([FromQuery] PageInputBase input,[FromQuery]string userId)
    {
       var q1 = _workLogRepository.AsQueryable().Where(x => x.CreatorUserId == userId && x.DeleteMark == null)
            .Select(x => new WorkLogPersonalOutput
            {
                id = x.Id,
                content = x.TodayContent,
                creatorTime = x.CreatorTime,
                type = "个人"
            });

        var q2 = _workLogRepository.Context.Queryable<TeamLogEntity>().Where(x => x.CreatorUserId == userId && x.DeleteMark == null)
            .Select(x => new WorkLogPersonalOutput
            {
                id = x.Id,
                content = x.Content,
                creatorTime = x.CreatorTime,
                type = "团队"
            });

        var q3 = _workLogRepository.Context.Queryable<ProjectLogEntity>().Where(x => x.CreatorUserId == userId && x.DeleteMark == null)
           .Select(x => new WorkLogPersonalOutput
           {
               id = x.Id,
               content = x.Content,
               creatorTime = x.CreatorTime,
               type = "项目"
           });

        var data = await _workLogRepository.Context.UnionAll(q1, q2, q3).OrderByDescending(a => a.creatorTime)
            .ToPagedListAsync(input.currentPage, input.pageSize);

        return PageResult<WorkLogPersonalOutput>.SqlSugarPagedList(data);
    }
    #endregion

    #region Post

    /// <summary>
    /// 添加.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] WorkLogCrInput input)
    {
        var entity = input.Adapt<WorkLogEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        //List<WorkLogShareEntity> workLogShareList = entity.ToUserId.Split(',').Select(x => new WorkLogShareEntity()
        //{
        //    Id = YitIdHelper.NextId().ToString(),
        //    ShareTime = DateTime.Now,
        //    WorkLogId = entity.Id,
        //    ShareUserId = x
        //}).ToList();

        //_workLogRepository.Context.Insertable(workLogShareList).ExecuteCommand();
        var isOk = await _workLogRepository.Context.Insertable(entity).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Update(string id, [FromBody] WorkLogUpInput input)
    {
        var entity = input.Adapt<WorkLogEntity>();
        //List<WorkLogShareEntity> workLogShareList = entity.ToUserId.Split(',').Select(x => new WorkLogShareEntity()
        //{
        //    Id = YitIdHelper.NextId().ToString(),
        //    ShareTime = DateTime.Now,
        //    WorkLogId = entity.Id,
        //    ShareUserId = x
        //}).ToList();

        if (entity.EnabledMark == 1)
        {
            // 判断是否为发布
            var workLogEntity = await _workLogRepository.Context.Queryable<WorkLogEntity>().Where(x => x.Id == id).FirstAsync() ?? throw Oops.Oh(ErrorCode.COM1005);
            if (workLogEntity.EnabledMark == 0)
            {
                // 判断是否为发布 , 更新创建时间
                entity.CreatorTime = DateTime.Now;
            }
        }
        //_db.BeginTran();
        _workLogRepository.Context.Deleteable<WorkLogShareEntity>().Where(x => x.WorkLogId == id).ExecuteCommand();
        //_workLogRepository.Context.Insertable(workLogShareList).ExecuteCommand();
        var isOk = await _workLogRepository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1001);

    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Delete(string id)
    {
        var entity = await _workLogRepository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        //_db.BeginTran();
        _workLogRepository.Context.Deleteable<WorkLogShareEntity>(x => x.WorkLogId == id).ExecuteCommand();
        var isOk = await _workLogRepository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.Delete()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1002);

    }
    #endregion
}
