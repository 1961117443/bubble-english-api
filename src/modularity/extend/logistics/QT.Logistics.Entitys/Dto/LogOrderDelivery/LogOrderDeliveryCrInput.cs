
using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogOrderDelivery;

/// <summary>
/// 出入库记录修改输入参数.
/// </summary>
public class LogOrderDeliveryCrInput
{
    /// <summary>
    /// 订单ID.
    /// </summary>
    public string orderId { get; set; }

    /// <summary>
    /// 配送点ID.
    /// </summary>
    public string pointId { get; set; }

    /// <summary>
    /// 仓库ID.
    /// </summary>
    public string storeRoomId { get; set; }

    /// <summary>
    /// 入库时间.
    /// </summary>
    public DateTime? inboundTime { get; set; }

    /// <summary>
    /// 入库人.
    /// </summary>
    public string inboundPerson { get; set; }

    /// <summary>
    /// 出库时间.
    /// </summary>
    public DateTime? outboundTime { get; set; }

    /// <summary>
    /// 出库人.
    /// </summary>
    public string outboundPerson { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

}

/// <summary>
/// 配送点扫码入库提交参数
/// </summary>
[SuppressSniffer]
public class LogOrderDeliveryInboundScanCrInput
{
    ///// <summary>
    ///// 配送点ID.
    ///// </summary>
    //public string pointId { get; set; }

    /// <summary>
    /// 仓库ID.
    /// </summary>
    public string storeRoomId { get; set; }

    public List<LogOrderDeliveryInboundScanOutput> list { get; set; }
}