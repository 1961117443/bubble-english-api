using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.DynamicApiController;
using QT.SDMS.Entitys.Dto.CustomerInvoice;
using QT.SDMS.Entitys.Entity;
using QT.Systems.Interfaces.System;
using SqlSugar;

namespace QT.SDMS;

/// <summary>
/// 业务实现：发票记录.
/// </summary>
[ApiDescriptionSettings("售电系统", Tag = "发票记录", Name = "CustomerInvoice", Order = 300)]
[Route("api/sdms/CustomerInvoice")]
public class CustomerInvoiceService : QTBaseService<CustomerInvoiceEntity, CustomerInvoiceCrInput, CustomerInvoiceUpInput, CustomerInvoiceOutput, CustomerInvoiceListQueryInput, CustomerInvoiceListOutput>, IDynamicApiController
{
    private readonly IBillRullService _billRullService;

    public CustomerInvoiceService(ISqlSugarRepository<CustomerInvoiceEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
    {
        _billRullService = billRullService;
    }

    protected override async Task BeforeCreate(CustomerInvoiceCrInput input, CustomerInvoiceEntity entity)
    {
        if (entity.InvoiceNo.IsNullOrEmpty())
        {
            entity.InvoiceNo = await _billRullService.GetBillNumber("SdmsCustomerInvoice");
        }
        
        await base.BeforeCreate(input, entity);
    }

    protected override async Task<SqlSugarPagedList<CustomerInvoiceListOutput>> GetPageList([FromQuery] CustomerInvoiceListQueryInput input)
    {
        //List<DateTime> queryInTime = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        //DateTime? startInTime = queryInTime?.First();
        //DateTime? endInTime = queryInTime?.Last();
        return await _repository.Context.Queryable<CustomerInvoiceEntity>()
            .Select<CustomerInvoiceListOutput>(it => new CustomerInvoiceListOutput
            {
                 customerIdName = SqlFunc.Subqueryable<CustomerEntity>().Where(ddd => ddd.Id == it.CustomerId).Select(ddd => ddd.Name),
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }
}