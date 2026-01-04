using QT.Common.Filter;

namespace QT.Iot.Application.Dto.MaintenancePlanOrder;

public class MaintenancePlanOrderListQueryInput:PageInputBase
{
    /// <summary>
    /// 入库订单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 入库类型.
    /// </summary>
    public string inType { get; set; }

    /// <summary>
    /// 入库日期.
    /// </summary>
    public string inTime { get; set; }
}
