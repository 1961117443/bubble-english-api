using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.DynamicApiController;
using QT.SDMS.Entitys.Dto.CustomerMeterPoint;
using QT.SDMS.Entitys.Entity;
using QT.Systems.Interfaces.System;
using SqlSugar;

namespace QT.SDMS;

/// <summary>
/// 业务实现：计量点表.
/// </summary>
[ApiDescriptionSettings("售电系统", Tag = "计量点表", Name = "CustomerMeterPoint", Order = 300)]
[Route("api/sdms/CustomerMeterPoint")]
public class CustomerMeterPointService : QTBaseService<CustomerMeterPointEntity, CustomerMeterPointCrInput, CustomerMeterPointUpInput, CustomerMeterPointOutput, CustomerMeterPointListQueryInput, CustomerMeterPointListOutput>, IDynamicApiController
{
    private readonly IBillRullService _billRullService;

    public CustomerMeterPointService(ISqlSugarRepository<CustomerMeterPointEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
    {
        _billRullService = billRullService;
    }

    protected override async Task<SqlSugarPagedList<CustomerMeterPointListOutput>> GetPageList([FromQuery] CustomerMeterPointListQueryInput input)
    {
        //List<DateTime> queryInTime = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        //DateTime? startInTime = queryInTime?.First();
        //DateTime? endInTime = queryInTime?.Last();
        return await _repository.Context.Queryable<CustomerMeterPointEntity>()
            .WhereIF(input.customerId.IsNotEmptyOrNull(), it => it.CustomerId == input.customerId)
            .Select<CustomerMeterPointListOutput>(it => new CustomerMeterPointListOutput
            {
                 customerIdName = SqlFunc.Subqueryable<CustomerEntity>().Where(ddd => ddd.Id == it.CustomerId).Select(ddd => ddd.Name),
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }
}