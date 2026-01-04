using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 订单装卸车记录实体.
/// </summary>
[SugarTable("log_order_classes")]
[Tenant(ClaimConst.TENANTID)]
public class LogOrderClassesEntity : CUDEntityBase
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
    /// 车次ID.
    /// </summary>
    [SugarColumn(ColumnName = "CId")]
    public string CId { get; set; }

    /// <summary>
    /// 装车时间.
    /// </summary>
    [SugarColumn(ColumnName = "InboundTime")]
    public DateTime? InboundTime { get; set; }

    /// <summary>
    /// 装车人.
    /// </summary>
    [SugarColumn(ColumnName = "InboundPerson")]
    public string InboundPerson { get; set; }

    /// <summary>
    /// 卸车时间.
    /// </summary>
    [SugarColumn(ColumnName = "OutboundTime")]
    public DateTime? OutboundTime { get; set; }

    /// <summary>
    /// 卸车人.
    /// </summary>
    [SugarColumn(ColumnName = "OutboundPerson")]
    public string OutboundPerson { get; set; }


    /// <summary>
    /// 批次号.
    /// </summary>
    [SugarColumn(ColumnName = "BatchNumber")]
    public string BatchNumber { get; set; }
}