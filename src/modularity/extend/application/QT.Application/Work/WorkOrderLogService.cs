using QT.Application.Entitys.Enum;
using QT.Common.Core;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Enum;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extend.Entitys;
using QT.Extend.Entitys.Dto.ProjectGantt;
using QT.Extend.Entitys.Dto.WorkOrderLog;
using QT.Message.Handlers;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;

namespace QT.Extend;

/// <summary>
    /// 工作日志
    /// </summary>
[ApiDescriptionSettings(Tag = "Extend", Name = "WorkOrderLog", Order = 600)]
[Route("api/extend/[controller]")]
public class WorkOrderLogService : QTBaseService<WorkOrderLogEntity, WorkOrderLogCrInput, WorkOrderLogUpInput, WorkOrderLogInfoOutput, WorkOrderLogListPageInput, WorkOrderLogListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<WorkOrderLogEntity> _repository;
    private readonly IUserManager _userManager;
    private readonly IBillRullService _billRullService;
    private readonly IMHandler _iMHandler;

    public WorkOrderLogService(ISqlSugarRepository<WorkOrderLogEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService, IMHandler iMHandler) : base(repository, context, userManager)
    {
        _repository = repository;
        _userManager = userManager;
        _billRullService = billRullService;
        _iMHandler = iMHandler;
    }

    private WorkOrderEntity _workOrderEntity;
    protected override async Task BeforeCreate(WorkOrderLogCrInput input, WorkOrderLogEntity entity)
    {
        _workOrderEntity = await _repository.Context.Queryable<WorkOrderEntity>().SingleAsync(x => x.Id == entity.Wid) ?? throw Oops.Oh(ErrorCode.COM1005);


        if (_workOrderEntity.CreatorUserId == _userManager.UserId)
        {
            // 提出人回复，收件人是处理人
            entity.ReceiveUserId = _workOrderEntity.AssignUserId;
        }
        else if (_workOrderEntity.AssignUserId == _userManager.UserId)
        {
            // 处理人回复，收件人是提出人
            entity.ReceiveUserId = _workOrderEntity.CreatorUserId;
        }
    }

    protected override async Task AfterCreate(WorkOrderLogEntity entity)
    {
        if (entity.ReceiveUserId.IsNotEmptyOrNull() && _workOrderEntity!=null)
        {
            string title = $"{_workOrderEntity.No}";
            if (_workOrderEntity.Category == (int)WorkOrderCategory.Project)
            {
                title = $"项目工单【{title}】";
            }
            if (_workOrderEntity.Category == (int)WorkOrderCategory.Team)
            {
                title = $"团队工单【{title}】";
            }
            if (_workOrderEntity.Category == (int)WorkOrderCategory.Personal)
            {
                title = $"个人工单【{title}】";
            }

            var body = new
            {
                method = "messageNotify",
                title = title,
                message = entity.Content,
                //dangerouslyUseHTMLString = true,
                //type = "info"
                redirectUrl= "/extend/apply/WorkOrder"
            };
            await _iMHandler.SendMessageToUserAsync(string.Format("{0}-{1}", TenantScoped.TenantId, entity.ReceiveUserId), body.ToJsonString());
        }
        
    }


    public override async Task<PageResult<WorkOrderLogListOutput>> GetList([FromQuery] WorkOrderLogListPageInput input)
    {
        var data = await _repository.Context.Queryable<WorkOrderLogEntity>()
            .WhereIF(input.wid.IsNotEmptyOrNull(), x => x.Wid == input.wid)
            .Select<WorkOrderLogListOutput>()
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<WorkOrderLogListOutput>.SqlSugarPagedList(data);
    }

    [HttpGet("Actions/{id}/List")]
    public async Task<List<WorkOrderLogListOutput>> GetList(string id)
    {
        var data = await _repository.Context.Queryable<WorkOrderLogEntity>()
            .Where(x => x.Wid == id)
            .OrderBy(x => x.CreatorTime)
            .Select<WorkOrderLogListOutput>()
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
