using QT.Common.Contracts;
using SqlSugar;

namespace QT.PRM.Entitys;

/// <summary>
/// 收费流水表
/// </summary>
[SugarTable("prm_payment")]
public class PaymentEntity : CLDEntityBase
{
    /// <summary>
    /// 应收款ID
    /// </summary>
    [SugarColumn(ColumnName = "receivable_id")]
    public string ReceivableId { get; set; }

    /// <summary>
    /// 金额
    /// </summary>
    [SugarColumn(DecimalDigits = 2)]
    public decimal Amount { get; set; }

    /// <summary>
    /// 支付方式
    /// </summary>
    [SugarColumn(ColumnName = "payment_method")]
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// 付款人类型
    /// </summary>
    [SugarColumn(ColumnName = "payer_type")]
    public PayerType PayerType { get; set; }

    /// <summary>
    /// 发票号
    /// </summary>
    [SugarColumn(ColumnName = "invoice_number", Length = 50)]
    public string InvoiceNumber { get; set; }
}


public enum PaymentMethod { 微信 = 1, 支付宝 = 2, 现金 = 3, 银行转账 = 4 }



public enum PayerType { 住户 = 1, 开发商 = 2, 第三方 = 3 }