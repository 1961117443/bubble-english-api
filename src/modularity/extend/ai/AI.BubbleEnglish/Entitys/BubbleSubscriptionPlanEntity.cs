namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
#region AI 评分记录
#endregion

[SugarTable("subscription_plan", TableDescription = "订阅/会员方案")]
public class BubbleSubscriptionPlanEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "方案ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "名称")]
    public string Name { get; set; }

    [SugarColumn(ColumnDescription = "价格（分）")]
    public int Price { get; set; }

    [SugarColumn(ColumnDescription = "有效天数")]
    public int Days { get; set; }

    [SugarColumn(ColumnDescription = "说明")]
    public string Description { get; set; }
}
