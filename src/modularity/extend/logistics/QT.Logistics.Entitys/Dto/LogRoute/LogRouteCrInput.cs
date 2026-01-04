using QT.Logistics.Entitys.Dto.LogDeliveryRoute;

namespace QT.Logistics.Entitys.Dto.LogRoute;

/// <summary>
/// 物流线路修改输入参数.
/// </summary>
public class LogRouteCrInput
{
    /// <summary>
    /// 线路名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 线路编号.
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// 线路覆盖配送点(中间表）.
    /// </summary>
    public List<LogDeliveryRouteCrInput> logDeliveryRouteList { get; set; }

}