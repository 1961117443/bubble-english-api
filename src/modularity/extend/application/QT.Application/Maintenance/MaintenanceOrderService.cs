using Microsoft.AspNetCore.Mvc;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core;
using QT.DynamicApiController;
using SqlSugar;
using QT.Iot.Application.Entity;
using QT.Iot.Application.Dto.MaintenanceOrder;
using QT.Common.Extension;
using QT.Systems.Interfaces.System;
using QT.Common.Security;
using QT.Systems.Entitys.Permission;

namespace QT.Iot.Application.Service;

/// <summary>
/// 业务实现：维保记录.
/// </summary>
[ApiDescriptionSettings("维保管理", Tag = "维保记录", Name = "MaintenanceOrder", Order = 300)]
[Route("api/iot/apply/maintenance-order")]
public class MaintenanceOrderService : QTBaseService<MaintenanceOrderEntity, MaintenanceRecordEntity, MaintenanceOrderCrInput, MaintenanceOrderUpInput, MaintenanceOrderOutput, MaintenanceOrderListQueryInput, MaintenanceOrderListOutput, MaintenanceRecordListOutput>, IDynamicApiController
{
    private readonly IBillRullService _billRullService;

    public MaintenanceOrderService(ISqlSugarRepository<MaintenanceOrderEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
    {
        _billRullService = billRullService;
    }

    protected override async Task BeforeCreate(MaintenanceOrderCrInput input, MaintenanceOrderEntity entity)
    {
        entity.No = await _billRullService.GetBillNumber("IotMaintenanceOrder");
        await base.BeforeCreate(input, entity);
    }

    public override async Task<MaintenanceOrderOutput> GetInfo(string id)
    {
        var data = await base.GetInfo(id);

        data.items = await _repository.Context.Queryable<MaintenanceRecordEntity>()
            .LeftJoin<MaterialEntity>((x, a) => x.Mid == a.Id)
            .Where(x => x.FId == id)
            .Select<MaintenanceRecordListOutput>((x, a) => new MaintenanceRecordListOutput
            {
                midName = a.Name,
                midCode = a.Code,
                midSpec = a.Spec,
                midUnit = a.Unit                 
            }, true).ToListAsync();

        return data;
    }

    protected override async Task<SqlSugarPagedList<MaintenanceOrderListOutput>> GetPageList([FromQuery] MaintenanceOrderListQueryInput input)
    {
        List<DateTime> queryInTime = input.inTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startInTime = queryInTime?.First();
        DateTime? endInTime = queryInTime?.Last();
        return await _repository.Context.Queryable<MaintenanceOrderEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(queryInTime != null, it => SqlFunc.Between(it.InTime, startInTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endInTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .Select<MaintenanceOrderListOutput>(it=> new MaintenanceOrderListOutput
            {
                creatorUserIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd=>ddd.Id == it.CreatorUserId).Select(ddd=>ddd.RealName),
                projectIdName = SqlFunc.Subqueryable<MaintenanceProjectEntity>().Where(ddd => ddd.Id == it.ProjectId).Select(ddd => ddd.Name)
            },true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }
}