using QT.Common.Filter;
using QT.SDMS.Entitys.Entity;

namespace QT.SDMS.Entitys.Dto.CustomerInvoice;

public class CustomerInvoiceListQueryInput:PageInputBase
{

    public string customerId { get; set; }
}
