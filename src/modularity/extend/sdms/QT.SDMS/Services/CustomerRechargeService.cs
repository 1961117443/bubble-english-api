using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.DynamicApiController;
using QT.SDMS.Entitys.Dto.CustomerRecharge;
using QT.SDMS.Entitys.Entity;
using QT.Systems.Interfaces.System;
using SqlSugar;

namespace QT.SDMS;

/// <summary>
/// 业务实现：充值记录.
/// </summary>
[ApiDescriptionSettings("售电系统", Tag = "充值记录", Name = "CustomerRecharge", Order = 300)]
[Route("api/sdms/CustomerRecharge")]
public class CustomerRechargeService : QTBaseService<CustomerRechargeEntity, CustomerRechargeCrInput, CustomerRechargeUpInput, CustomerRechargeOutput, CustomerRechargeListQueryInput, CustomerRechargeListOutput>, IDynamicApiController
{
    private readonly IBillRullService _billRullService;

    public CustomerRechargeService(ISqlSugarRepository<CustomerRechargeEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
    {
        _billRullService = billRullService;
    }

    protected override async Task BeforeCreate(CustomerRechargeCrInput input, CustomerRechargeEntity entity)
    {
        entity.RechargeNo = await _billRullService.GetBillNumber("SdmsCustomerRecharge");
        await base.BeforeCreate(input, entity);
    }

    protected override async Task<SqlSugarPagedList<CustomerRechargeListOutput>> GetPageList([FromQuery] CustomerRechargeListQueryInput input)
    {
        //List<DateTime> queryInTime = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        //DateTime? startInTime = queryInTime?.First();
        //DateTime? endInTime = queryInTime?.Last();
        return await _repository.Context.Queryable<CustomerRechargeEntity>()
            .WhereIF(input.customerId.IsNotEmptyOrNull(), it => it.CustomerId == input.customerId)
            .Select<CustomerRechargeListOutput>(it => new CustomerRechargeListOutput
            {
                 customerIdName = SqlFunc.Subqueryable<CustomerEntity>().Where(ddd => ddd.Id == it.CustomerId).Select(ddd => ddd.Name),
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }
}