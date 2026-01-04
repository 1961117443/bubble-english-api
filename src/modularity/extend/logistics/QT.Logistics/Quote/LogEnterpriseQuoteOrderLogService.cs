using QT.Common.Core;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Security;
using QT.Logistics.Entitys;
using QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrderLog;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;

namespace QT.Logistics;

/// <summary>
    /// 报价单沟通记录
    /// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "报价单沟通记录", Name = "LogEnterpriseQuoteOrderLog", Order = 600)]
[Route("api/Logistics/quote/[controller]")]
public class LogEnterpriseQuoteOrderLogService : QTBaseService<LogEnterpriseQuoteOrderLogEntity, LogEnterpriseQuoteOrderLogCrInput, LogEnterpriseQuoteOrderLogUpInput, LogEnterpriseQuoteOrderLogInfoOutput, LogEnterpriseQuoteOrderLogListPageInput, LogEnterpriseQuoteOrderLogListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<LogEnterpriseQuoteOrderLogEntity> _repository;
    private readonly IUserManager _userManager;
    private readonly IBillRullService _billRullService;

    public LogEnterpriseQuoteOrderLogService(ISqlSugarRepository<LogEnterpriseQuoteOrderLogEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
    {
        _repository = repository;
        _userManager = userManager;
        _billRullService = billRullService;
    }



    public override async Task<PageResult<LogEnterpriseQuoteOrderLogListOutput>> GetList([FromQuery] LogEnterpriseQuoteOrderLogListPageInput input)
    {
        var data = await _repository.Context.Queryable<LogEnterpriseQuoteOrderLogEntity>()
            .WhereIF(input.fid.IsNotEmptyOrNull(), x => x.Fid == input.fid)
            .Select<LogEnterpriseQuoteOrderLogListOutput>()
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogEnterpriseQuoteOrderLogListOutput>.SqlSugarPagedList(data);
    }

    [HttpGet("Actions/{id}/List")]
    public async Task<List<LogEnterpriseQuoteOrderLogListOutput>> GetList(string id)
    {
        var data = await _repository.Context.Queryable<LogEnterpriseQuoteOrderLogEntity>()
            .Where(x => x.Fid == id)
            .OrderBy(x => x.CreatorTime)
            .Select<LogEnterpriseQuoteOrderLogListOutput>()
            .ToListAsync();

        var userIds = data.Select(x => x.creatorUserId).ToList();
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

            foreach (var item in data)
            {
                item.managerInfo = users.Find(x => x.Id == item.creatorUserId)?.Adapt<ManagersInfo>() ?? new ManagersInfo();
            }
        }
        return data;
    }

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
