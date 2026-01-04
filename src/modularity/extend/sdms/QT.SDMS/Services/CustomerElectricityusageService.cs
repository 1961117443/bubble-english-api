using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Filter;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.DynamicApiController;
using QT.SDMS.Entitys.Dto.CustomerElectricityusage;
using QT.SDMS.Entitys.Entity;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;
using SqlSugar;

namespace QT.SDMS;

/// <summary>
/// 业务实现：用电分时数据.
/// </summary>
[ApiDescriptionSettings("售电系统", Tag = "用电分时数据", Name = "CustomerElectricityusage", Order = 300)]
[Route("api/sdms/CustomerElectricityusage")]
public class CustomerElectricityusageService : QTBaseService<CustomerElectricityusageEntity, CustomerElectricityusageCrInput, CustomerElectricityusageUpInput, CustomerElectricityusageOutput, CustomerElectricityusageListQueryInput, CustomerElectricityusageListOutput>, IDynamicApiController
{

    public CustomerElectricityusageService(ISqlSugarRepository<CustomerElectricityusageEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
    }

    protected override async Task<SqlSugarPagedList<CustomerElectricityusageListOutput>> GetPageList([FromQuery] CustomerElectricityusageListQueryInput input)
    {
        //List<DateTime> queryInTime = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        //DateTime? startInTime = queryInTime?.First();
        //DateTime? endInTime = queryInTime?.Last();
        return await _repository.Context.Queryable<CustomerElectricityusageEntity>()
            .InnerJoin<CustomerMeterPointEntity>((it, a) => it.MeterPointId == a.Id)
            .WhereIF(input.customerId.IsNotEmptyOrNull(), (it,a) => a.CustomerId == input.customerId)
            //.WhereIF(!string.IsNullOrEmpty(input.keyword), it => it.No.Contains(input.keyword))
            //.WhereIF(queryInTime != null, it => SqlFunc.Between(it.OrderDate, startInTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endInTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .Select<CustomerElectricityusageListOutput>((it, a) => new CustomerElectricityusageListOutput
            {
                customerIdName = SqlFunc.Subqueryable<CustomerEntity>().Where(ddd => ddd.Id == a.CustomerId).Select(ddd => ddd.Name),
                meterPointIdCode = a.MeterCode
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }
}