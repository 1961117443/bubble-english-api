using QT.Common.Filter;
using QT.SDMS.Entitys.Entity;

namespace QT.SDMS.Entitys.Dto.CustomerElectricityusage;

public class CustomerElectricityusageListQueryInput:PageInputBase
{

    /// <summary>
    /// 客户id
    /// </summary>
    public string customerId { get; set; }

    /// <summary>
    /// 计量点
    /// </summary>
    public string meterPointId { get; set; }
}
