namespace QT.Logistics.Entitys.Dto.LogOrderDelivery;

/// <summary>
/// 出入库记录输入参数.
/// </summary>
public class LogOrderDeliveryListOutput
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
    /// 仓库ID.
    /// </summary>
    public string storeRoomIdName { get; set; }

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
    /// 装车人姓名.
    /// </summary>
    public string inboundPersonName { get; set; }

    /// <summary>
    /// 卸车人姓名.
    /// </summary>
    public string outboundPersonName { get; set; }
}