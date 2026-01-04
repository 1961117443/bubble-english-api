
using QT.DependencyInjection;
using QT.Logistics.Entitys.Dto.LogOrderDelivery;

namespace QT.Logistics.Entitys.Dto.LogOrderClasses;

/// <summary>
/// 订单装卸车记录修改输入参数.
/// </summary>
public class LogOrderClassesCrInput
{
    /// <summary>
    /// 订单ID.
    /// </summary>
    public string orderId { get; set; }

    /// <summary>
    /// 车次ID.
    /// </summary>
    public string cId { get; set; }

    /// <summary>
    /// 装车时间.
    /// </summary>
    public DateTime? inboundTime { get; set; }

    /// <summary>
    /// 装车人.
    /// </summary>
    public string inboundPerson { get; set; }

    /// <summary>
    /// 卸车时间.
    /// </summary>
    public DateTime? outboundTime { get; set; }

    /// <summary>
    /// 卸车人.
    /// </summary>
    public string outboundPerson { get; set; }

}

/// <summary>
/// 扫码装车提交参数
/// </summary>
[SuppressSniffer]
public class LogOrderClassesInboundScanCrInput: LogOrderClassesOutboundScanCrInput
{
    /// <summary>
    /// 车次ID.
    /// </summary>
    public string cId { get; set; }
}

/// <summary>
/// 扫码卸车提交参数
/// </summary>
[SuppressSniffer]
public class LogOrderClassesOutboundScanCrInput
{
    /// <summary>
    /// 经度.
    /// </summary>
    public string longitude { get; set; }

    /// <summary>
    /// 纬度.
    /// </summary>
    public string latitude { get; set; }


    public List<LogOrderDeliveryInboundScanOutput> list { get; set; }
}