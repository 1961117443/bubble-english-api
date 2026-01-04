using QT.Common.Core;
using QT.Common.Enum;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extend.Entitys.Dto.ProjectGantt;
using QT.Extend.Entitys.Dto.QuoteOrderLog;
using QT.Message.Handlers;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;

namespace QT.Extend;

/// <summary>
    /// 报价单沟通记录
    /// </summary>
[ApiDescriptionSettings("扩展应用", Tag = "报价单沟通记录", Name = "QuoteOrderLog", Order = 600)]
[Route("api/extend/quote/[controller]")]
public class QuoteOrderLogService : QTBaseService<QuoteOrderLogEntity, QuoteOrderLogCrInput, QuoteOrderLogUpInput, QuoteOrderLogInfoOutput, QuoteOrderLogListPageInput, QuoteOrderLogListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<QuoteOrderLogEntity> _repository;
    private readonly IUserManager _userManager;
    private readonly IBillRullService _billRullService;
    private readonly IMHandler _iMHandler;

    public QuoteOrderLogService(ISqlSugarRepository<QuoteOrderLogEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService, IMHandler iMHandler) : base(repository, context, userManager)
    {
        _repository = repository;
        _userManager = userManager;
        _billRullService = billRullService;
        _iMHandler = iMHandler;
    }



    public override async Task<PageResult<QuoteOrderLogListOutput>> GetList([FromQuery] QuoteOrderLogListPageInput input)
    {
        var data = await _repository.Context.Queryable<QuoteOrderLogEntity>()
            .WhereIF(input.fid.IsNotEmptyOrNull(), x => x.Fid == input.fid)
            .Select<QuoteOrderLogListOutput>()
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<QuoteOrderLogListOutput>.SqlSugarPagedList(data);
    }

    [HttpGet("Actions/{id}/List")]
    public async Task<List<QuoteOrderLogListOutput>> GetList(string id)
    {
        var data = await _repository.Context.Queryable<QuoteOrderLogEntity>()
            .Where(x => x.Fid == id)
            .OrderBy(x => x.CreatorTime)
            .Select<QuoteOrderLogListOutput>()
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
