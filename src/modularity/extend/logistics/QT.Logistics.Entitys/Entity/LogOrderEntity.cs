using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 订单管理实体.
/// </summary>
[SugarTable("log_order")]
[Tenant(ClaimConst.TENANTID)]
public class LogOrderEntity : CUDEntityBase, IDeleteTime
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    [SugarColumn(ColumnName = "OrderNo")]
    public string OrderNo { get; set; }

    /// <summary>
    /// 寄件配送点id.
    /// </summary>
    [SugarColumn(ColumnName = "SendPointId")]
    public string SendPointId { get; set; }

    /// <summary>
    /// 送达配送点id.
    /// </summary>
    [SugarColumn(ColumnName = "ReachPointId")]
    public string ReachPointId { get; set; }

    /// <summary>
    /// 订单日期.
    /// </summary>
    [SugarColumn(ColumnName = "OrderDate")]
    public DateTime? OrderDate { get; set; }

    /// <summary>
    /// 订单状态.
    /// </summary>
    [SugarColumn(ColumnName = "OrderStatus")]
    public int? OrderStatus { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 寄件人姓名.
    /// </summary>
    [SugarColumn(ColumnName = "ShipperName")]
    public string ShipperName { get; set; }

    /// <summary>
    /// 寄件人电话.
    /// </summary>
    [SugarColumn(ColumnName = "ShipperPhone")]
    public string ShipperPhone { get; set; }

    /// <summary>
    /// 寄件地址.
    /// </summary>
    [SugarColumn(ColumnName = "ShipperAddress")]
    public string ShipperAddress { get; set; }

    /// <summary>
    /// 收件人姓名.
    /// </summary>
    [SugarColumn(ColumnName = "RecipientName")]
    public string RecipientName { get; set; }

    /// <summary>
    /// 收件人电话.
    /// </summary>
    [SugarColumn(ColumnName = "RecipientPhone")]
    public string RecipientPhone { get; set; }

    /// <summary>
    /// 收件地址.
    /// </summary>
    [SugarColumn(ColumnName = "RecipientAddress")]
    public string RecipientAddress { get; set; }

    /// <summary>
    /// 订单运费.
    /// </summary>
    [SugarColumn(ColumnName = "Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 分账人id.
    /// </summary>
    [SugarColumn(ColumnName = "AccountUserId")]
    public string AccountUserId { get; set; }

    /// <summary>
    /// 分账时间.
    /// </summary>
    [SugarColumn(ColumnName = "AccountTime")]
    public DateTime? AccountTime { get; set; }


    /// <summary>
    /// 平台分成.
    /// </summary>
    [SugarColumn(ColumnName = "PlatformAmount")]
    public decimal PlatformAmount { get; set; }

    /// <summary>
    /// 收件点分成.
    /// </summary>
    [SugarColumn(ColumnName = "SendPointAmount")]
    public decimal SendPointAmount { get; set; }

    /// <summary>
    /// 到达点分成.
    /// </summary>
    [SugarColumn(ColumnName = "ReachPointAmount")]
    public decimal ReachPointAmount { get; set; }


    /// <summary>
    /// 结算人id.
    /// </summary>
    [SugarColumn(ColumnName = "SettlementUserId")]
    public string SettlementUserId { get; set; }

    /// <summary>
    /// 结算时间.
    /// </summary>
    [SugarColumn(ColumnName = "SettlementTime")]
    public DateTime? SettlementTime { get; set; }
}