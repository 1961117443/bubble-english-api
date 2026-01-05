namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
using System;

/// <summary>
/// 主题（zoo/farm/body...）
/// </summary>
[SugarTable("bubble_theme", TableDescription = "Bubble 主题")]
public class BubbleThemeEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "主题ID")]
    public long Id { get; set; }

    public string ThemeKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Sort { get; set; } = 0;
    public int Enabled { get; set; } = 1;
    public DateTime CreateTime { get; set; } = DateTime.Now;
}
