using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 订单财务明细实体.
/// </summary>
[SugarTable("log_order_financial")]
[Tenant(ClaimConst.TENANTID)]
public class LogOrderFinancialEntity : CUDEntityBase,IDeleteTime
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 订单id.
    /// </summary>
    [SugarColumn(ColumnName = "OrderId")]
    public string OrderId { get; set; }

    /// <summary>
    /// 运费.
    /// </summary>
    [SugarColumn(ColumnName = "Freight")]
    public decimal Freight { get; set; }

    /// <summary>
    /// 支付方式.
    /// </summary>
    [SugarColumn(ColumnName = "PaymentMethod")]
    public string PaymentMethod { get; set; }

    /// <summary>
    /// 收款账号.
    /// </summary>
    [SugarColumn(ColumnName = "ReceivingAccount")]
    public string ReceivingAccount { get; set; }

    /// <summary>
    /// 收款时间.
    /// </summary>
    [SugarColumn(ColumnName = "ReceivingTime")]
    public DateTime? ReceivingTime { get; set; }

    /// <summary>
    /// 实际到款.
    /// </summary>
    [SugarColumn(ColumnName = "ActualPayment")]
    public decimal ActualPayment { get; set; }

    /// <summary>
    /// 手续费.
    /// </summary>
    [SugarColumn(ColumnName = "HandlingFee")]
    public decimal HandlingFee { get; set; }

    /// <summary>
    /// 支付账号.
    /// </summary>
    [SugarColumn(ColumnName = "PaymentAccount")]
    public string PaymentAccount { get; set; }

    /// <summary>
    /// 结算状态.
    /// </summary>
    [SugarColumn(ColumnName = "SettlementStatus")]
    public int? SettlementStatus { get; set; }

    /// <summary>
    /// 结算时间.
    /// </summary>
    [SugarColumn(ColumnName = "SettlementTime")]
    public DateTime? SettlementTime { get; set; }
}