
using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogOrderDelivery;

/// <summary>
/// 出入库记录输出参数.
/// </summary>
public class LogOrderDeliveryInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 订单ID.
    /// </summary>
    public string orderId { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string orderIdOrderNo { get; set; }

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



    /// <summary>
    /// 状态（0：入库，1：出库，2：完成）.
    /// </summary>
    public int status { get; set; }


    /// <summary>
    /// 入库人.
    /// </summary>
    public string inboundPersonName { get; set; }

}

/// <summary>
/// 扫码入库输出实体
/// </summary>
[SuppressSniffer]
public class LogOrderDeliveryInboundScanOutput
{
    /// <summary>
    /// 订单id
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 分类（order | classes）
    /// </summary>
    public InboundScanEnum category { get; set; }
}

public enum InboundScanEnum
{
    /// <summary>
    /// 物流订单
    /// </summary>
    order,
    /// <summary>
    /// 车次
    /// </summary>
    classes,
    /// <summary>
    /// 配送清单
    /// </summary>
    note
}

public enum DeliveryInboundScanEnum
{
    /// <summary>
    /// 物流订单
    /// </summary>
    order,
    /// <summary>
    /// 仓库位置
    /// </summary>
    storeroom,
    /// <summary>
    /// 配送清单
    /// </summary>
    note
}