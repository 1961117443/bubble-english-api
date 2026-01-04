using QT.Common.Filter;
using QT.SDMS.Entitys.Entity;

namespace QT.SDMS.Entitys.Dto.CustomerRefund;

public class CustomerRefundListQueryInput:PageInputBase
{

    public string customerId { get; set; }
}
