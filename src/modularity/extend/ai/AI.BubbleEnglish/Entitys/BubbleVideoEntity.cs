namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
using System;

/// <summary>
/// 视频资源（后台上传）
/// </summary>
[SugarTable("bubble_video", TableDescription = "Bubble 视频库")]
public class BubbleVideoEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "视频ID")]
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string ThemeKey { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;
    public int DurationSec { get; set; }
    public string Status { get; set; } = "uploaded"; // uploaded/analyzing/done/failed
    public long? AnalyzeJobId { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;
    public DateTime UpdateTime { get; set; } = DateTime.Now;
}
