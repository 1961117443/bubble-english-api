namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
using System;

/// <summary>
/// Unit 素材池（word/sentence/knowledge）
/// </summary>
[SugarTable("bubble_unit", TableDescription = "Bubble Unit 素材池")]
public class BubbleUnitEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "UnitID")]
    public long Id { get; set; }

    public long? VideoId { get; set; }
    public string UnitType { get; set; } = "word"; // word/sentence/knowledge
    public string Text { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public int MinAge { get; set; } = 3;
    public string ImageUrl { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public string Status { get; set; } = "draft"; // draft/published
    public DateTime CreateTime { get; set; } = DateTime.Now;
    public DateTime UpdateTime { get; set; } = DateTime.Now;
}
