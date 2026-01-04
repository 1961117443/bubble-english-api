using QT.Logistics.Entitys.Dto.LogDeliveryRoute;

namespace QT.Logistics.Entitys.Dto.LogRoute;

/// <summary>
/// 物流线路输出参数.
/// </summary>
public class LogRouteInfoOutput
{
    /// <summary>
    /// 自然主键.
    /// </summary>
    public string id { get; set; }

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
    public List<LogDeliveryRouteInfoOutput> logDeliveryRouteList { get; set; }

}