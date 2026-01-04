using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 出入库记录实体.
/// </summary>
[SugarTable("log_order_delivery")]
[Tenant(ClaimConst.TENANTID)]
public class LogOrderDeliveryEntity : CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 订单ID.
    /// </summary>
    [SugarColumn(ColumnName = "OrderId")]
    public string OrderId { get; set; }

    /// <summary>
    /// 配送点ID.
    /// </summary>
    [SugarColumn(ColumnName = "PointId")]
    public string PointId { get; set; }

    /// <summary>
    /// 仓库ID.
    /// </summary>
    [SugarColumn(ColumnName = "StoreRoomId")]
    public string StoreRoomId { get; set; }

    /// <summary>
    /// 入库时间.
    /// </summary>
    [SugarColumn(ColumnName = "InboundTime")]
    public DateTime? InboundTime { get; set; }

    /// <summary>
    /// 入库人.
    /// </summary>
    [SugarColumn(ColumnName = "InboundPerson")]
    public string InboundPerson { get; set; }

    /// <summary>
    /// 出库时间.
    /// </summary>
    [SugarColumn(ColumnName = "OutboundTime")]
    public DateTime? OutboundTime { get; set; }

    /// <summary>
    /// 出库人.
    /// </summary>
    [SugarColumn(ColumnName = "OutboundPerson")]
    public string OutboundPerson { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }

}