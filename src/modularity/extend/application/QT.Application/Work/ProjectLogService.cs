using DingTalk.Api.Request;
using QT.Common.Core;
using QT.Common.Enum;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extend.Entitys.Dto.ProjectGantt;
using QT.Extend.Entitys.Dto.ProjectLog;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;

namespace QT.Extend;

/// <summary>
    /// 工作日志
    /// </summary>
[ApiDescriptionSettings(Tag = "Extend", Name = "ProjectLog", Order = 600)]
[Route("api/extend/[controller]")]
public class ProjectLogService : QTBaseService<ProjectLogEntity,ProjectLogCrInput,ProjectLogUpInput,ProjectLogInfoOutput,ProjectLogListPageInput,ProjectLogListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<ProjectLogEntity> _repository;

    public ProjectLogService(ISqlSugarRepository<ProjectLogEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }

    #region Get

    /// <summary>
    /// 列表(团队日志列表)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public override async Task<PageResult<ProjectLogListOutput>> GetList([FromQuery] ProjectLogListPageInput input)
    {
        var list = await _repository.AsQueryable().Where(x => x.ProjectId == input.projectId && x.DeleteMark == null)
            .WhereIF(input.keyword.IsNotEmptyOrNull(), m => m.Content.Contains(input.keyword))
            .WhereIF(input.date.IsNotEmptyOrNull(), m=> SqlFunc.Between(m.CreatorTime,input.date, $"{input.date} 23:59:59"))
            .OrderBy(x => x.CreatorTime, OrderByType.Desc)
            .OrderByIF(!string.IsNullOrEmpty(input.keyword), t => t.LastModifyTime, OrderByType.Desc)
            .Select(x => new ProjectLogListOutput
            {
                sender = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == x.CreatorUserId).Select(ddd => ddd.RealName),
                sendTime = x.CreatorTime,
                creatorUserId = x.CreatorUserId
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);

        var userIds = list.list.Select(x => x.creatorUserId).ToList();
        if (userIds.IsAny())
        {
            var users = await _repository.Context.Queryable<UserEntity>().Where(x => userIds.Contains(x.Id))
                .Select(x => new UserEntity
                {
                    Id = x.Id,
                    RealName = x.RealName,
                    Account = x.Account,
                    HeadIcon = x.HeadIcon,
                })
                .ToListAsync();

            foreach (var item in list.list)
            {
                item.managerInfo = users.Find(x => x.Id == item.creatorUserId)?.Adapt<ManagersInfo>() ?? new ManagersInfo();
            }
        }

        return PageResult<ProjectLogListOutput>.SqlSugarPagedList(list);
    }

    /// <summary>
    /// 按日期汇总日志数据
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    [HttpGet("Actions/{projectId}/LogSumByDay")]
    public async Task<List<ProjectLogSumOutput>> LogSumByDay(string projectId)
    {
        var list = await _repository.AsQueryable().Where(x => x.ProjectId == projectId && x.DeleteMark == null)

            .Select(x => new ProjectLogSumOutput
            {
                date = x.CreatorTime.Value.ToString("yyyy-MM-dd")
            })
            .MergeTable()
            .GroupBy(x => x.date)
            .Select(x => new ProjectLogSumOutput
            {
                date = x.date,
                count = SqlFunc.AggregateCount(x.date)
            })
            .OrderByDescending(x=>x.date)
            .ToListAsync();

        return list;
    }
    #endregion

    /// <summary>
    /// 删除.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public override async Task Delete(string id)
    {
        var entity = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        _repository.Context.Tracking(entity);
        entity.Delete();

        await _repository.UpdateAsync(entity);
    }
}
