using Microsoft.AspNetCore.Mvc;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core;
using QT.DynamicApiController;
using SqlSugar;
using QT.Iot.Application.Entity;
using QT.Iot.Application.Dto.MaintenancePlanOrder;
using QT.Common.Extension;
using QT.Systems.Interfaces.System;
using QT.Common.Security;
using QT.Systems.Entitys.Permission;

namespace QT.Iot.Application.Service;

/// <summary>
/// 业务实现：维保计划.
/// </summary>
[ApiDescriptionSettings("维保管理", Tag = "维保记录", Name = "MaintenancePlanOrder", Order = 300)]
[Route("api/iot/apply/maintenance-plan-order")]
public class MaintenancePlanOrderService : QTBaseService<MaintenancePlanOrderEntity, MaintenancePlanRecordEntity, MaintenancePlanOrderCrInput, MaintenancePlanOrderUpInput, MaintenancePlanOrderOutput, MaintenancePlanOrderListQueryInput, MaintenancePlanOrderListOutput, MaintenancePlanRecordListOutput>, IDynamicApiController
{
    private readonly IBillRullService _billRullService;

    public MaintenancePlanOrderService(ISqlSugarRepository<MaintenancePlanOrderEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
    {
        _billRullService = billRullService;
    }

    protected override async Task BeforeCreate(MaintenancePlanOrderCrInput input, MaintenancePlanOrderEntity entity)
    {
        entity.No = await _billRullService.GetBillNumber("IotMaintenancePlan");
        await base.BeforeCreate(input, entity);
    }

    public override async Task<MaintenancePlanOrderOutput> GetInfo(string id)
    {
        var data = await base.GetInfo(id);

        data.items = await _repository.Context.Queryable<MaintenancePlanRecordEntity>()
            .LeftJoin<MaterialEntity>((x, a) => x.Mid == a.Id)
            .Where(x => x.FId == id)
            .Select<MaintenancePlanRecordListOutput>((x, a) => new MaintenancePlanRecordListOutput
            {
                midName = a.Name,
                midCode = a.Code,
                midSpec = a.Spec,
                midUnit = a.Unit                 
            }, true).ToListAsync();

        return data;
    }

    protected override async Task<SqlSugarPagedList<MaintenancePlanOrderListOutput>> GetPageList([FromQuery] MaintenancePlanOrderListQueryInput input)
    {
        List<DateTime> queryInTime = input.inTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startInTime = queryInTime?.First();
        DateTime? endInTime = queryInTime?.Last();
        return await _repository.Context.Queryable<MaintenancePlanOrderEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(queryInTime != null, it => SqlFunc.Between(it.InTime, startInTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endInTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .Select<MaintenancePlanOrderListOutput>(it=> new MaintenancePlanOrderListOutput
            {
                creatorUserIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd=>ddd.Id == it.CreatorUserId).Select(ddd=>ddd.RealName),
                taskUserIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == it.TaskUserId).Select(ddd => ddd.RealName),
                projectIdName = SqlFunc.Subqueryable<MaintenanceProjectEntity>().Where(ddd => ddd.Id == it.ProjectId).Select(ddd => ddd.Name)
            },true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }
}