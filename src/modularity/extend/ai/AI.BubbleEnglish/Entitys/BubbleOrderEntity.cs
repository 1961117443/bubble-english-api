namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
using System;
#region 订阅方案
#endregion

[SugarTable("order", TableDescription = "订单表")]
public class BubbleOrderEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "订单ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "用户ID")]
    public long UserId { get; set; }

    [SugarColumn(ColumnDescription = "方案ID")]
    public long PlanId { get; set; }

    [SugarColumn(ColumnDescription = "金额（分）")]
    public int Amount { get; set; }

    [SugarColumn(ColumnDescription = "状态 pending/paid/cancel")]
    public string Status { get; set; }

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreateTime { get; set; }
}
