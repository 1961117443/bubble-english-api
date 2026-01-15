namespace AI.BubbleEnglish.Entitys;

using QT.Common.Contracts;
using SqlSugar;
using System;

/// <summary>
/// 主题（zoo/farm/body...）
/// </summary>
[SugarTable("bubble_theme", TableDescription = "Bubble 主题")]
public class BubbleThemeEntity : EntityBase<string>
{
    [SugarColumn(IsPrimaryKey = true, ColumnDescription = "主题ID")]
    public override string Id { get; set; }

    [SugarColumn(ColumnDescription = "主题 Key")]
    public string ThemeKey { get; set; } = string.Empty;

    [SugarColumn(ColumnDescription = "标题")]
    public string Title { get; set; } = string.Empty;

    [SugarColumn(ColumnDescription = "封面地址")]
    public string CoverUrl { get; set; } = string.Empty;

    [SugarColumn(ColumnDescription = "描述")]
    public string Description { get; set; } = string.Empty;

    [SugarColumn(ColumnDescription = "排序")]
    public int Sort { get; set; } = 0;

    [SugarColumn(ColumnDescription = "是否启用 1启用 0禁用")]
    public int Enabled { get; set; } = 1;

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreateTime { get; set; } = DateTime.Now;
}
