namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
using System;
#region 订单
#endregion

[SugarTable("payment_record", TableDescription = "支付记录")]
public class BubblePaymentRecordEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "支付记录ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "订单ID")]
    public long OrderId { get; set; }

    [SugarColumn(ColumnDescription = "微信支付单号")]
    public string TransactionId { get; set; }

    [SugarColumn(ColumnDescription = "支付金额（分）")]
    public int Amount { get; set; }

    [SugarColumn(ColumnDescription = "支付时间")]
    public DateTime PayTime { get; set; }
}
#endregion
