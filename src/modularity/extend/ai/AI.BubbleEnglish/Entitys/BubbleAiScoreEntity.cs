namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
using System;
#region 学习记录
#endregion

[SugarTable("ai_score", TableDescription = "AI 朗读评分记录")]
public class BubbleAiScoreEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "评分ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "孩子ID")]
    public long ChildId { get; set; }

    [SugarColumn(ColumnDescription = "课时ID")]
    public long LessonId { get; set; }

    [SugarColumn(ColumnDescription = "原文内容")]
    public string Text { get; set; }

    [SugarColumn(ColumnDescription = "分数（0-100）")]
    public int Score { get; set; }

    [SugarColumn(ColumnDescription = "AI 详细 JSON")]
    public string Detail { get; set; }

    [SugarColumn(ColumnDescription = "朗读音频地址")]
    public string AudioUrl { get; set; }

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreateTime { get; set; }
}
#endregion
