using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.DynamicApiController;
using QT.SDMS.Entitys.Dto.CustomerRefund;
using QT.SDMS.Entitys.Entity;
using QT.Systems.Interfaces.System;
using SqlSugar;

namespace QT.SDMS;

/// <summary>
/// 业务实现：充值记录.
/// </summary>
[ApiDescriptionSettings("售电系统", Tag = "退款记录", Name = "CustomerRefund", Order = 300)]
[Route("api/sdms/CustomerRefund")]
public class CustomerRefundService : QTBaseService<CustomerRefundEntity, CustomerRefundCrInput, CustomerRefundUpInput, CustomerRefundOutput, CustomerRefundListQueryInput, CustomerRefundListOutput>, IDynamicApiController
{
    private readonly IBillRullService _billRullService;

    public CustomerRefundService(ISqlSugarRepository<CustomerRefundEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
    {
        _billRullService = billRullService;
    }

    protected override async Task BeforeCreate(CustomerRefundCrInput input, CustomerRefundEntity entity)
    {
        entity.RefundNo = await _billRullService.GetBillNumber("SdmsCustomerRefund");
        await base.BeforeCreate(input, entity);
    }

    protected override async Task<SqlSugarPagedList<CustomerRefundListOutput>> GetPageList([FromQuery] CustomerRefundListQueryInput input)
    {
        //List<DateTime> queryInTime = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        //DateTime? startInTime = queryInTime?.First();
        //DateTime? endInTime = queryInTime?.Last();
        return await _repository.Context.Queryable<CustomerRefundEntity>()
            .Select<CustomerRefundListOutput>(it => new CustomerRefundListOutput
            {
                 customerIdName = SqlFunc.Subqueryable<CustomerEntity>().Where(ddd => ddd.Id == it.CustomerId).Select(ddd => ddd.Name),
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }
}