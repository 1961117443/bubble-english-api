using QT.Common.Filter;
using QT.SDMS.Entitys.Entity;

namespace QT.SDMS.Entitys.Dto.CustomerMeterPoint;

public class CustomerMeterPointListQueryInput:PageInputBase
{

    public string customerId { get; set; }
}
