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

    [SugarColumn(ColumnDescription = "关联视频ID")]
    public long? VideoId { get; set; }

    [SugarColumn(ColumnDescription = "Unit 类型：word/sentence/knowledge")]
    public string UnitType { get; set; } // word/sentence/knowledge

    [SugarColumn(ColumnDescription = "文本内容")]
    public string Text { get; set; }

    [SugarColumn(ColumnDescription = "释义/含义")]
    public string Meaning { get; set; }

    [SugarColumn(ColumnDescription = "最小年龄")]
    public int MinAge { get; set; }

    [SugarColumn(ColumnDescription = "图片地址")]
    public string ImageUrl { get; set; }

    [SugarColumn(ColumnDescription = "音频地址")]
    public string AudioUrl { get; set; }

    [SugarColumn(ColumnDescription = "状态：draft/published")]
    public string Status { get; set; } // draft/published

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreateTime { get; set; }

    [SugarColumn(ColumnDescription = "更新时间")]
    public DateTime UpdateTime { get; set; }
}
