using QT.Common.Filter;
using QT.SDMS.Entitys.Entity;

namespace QT.SDMS.Entitys.Dto.CustomerRecharge;

public class CustomerRechargeListQueryInput:PageInputBase
{

    public string customerId { get; set; }
}
