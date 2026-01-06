namespace AI.BubbleEnglish.Entitys;

using QT.Common.Contracts;
using SqlSugar;
using System;

/// <summary>
/// 视频资源（后台上传）
/// </summary>
[SugarTable("bubble_video", TableDescription = "Bubble 视频库")]
public class BubbleVideoEntity :EntityBase<string>
{
    [SugarColumn(IsPrimaryKey = true, ColumnDescription = "视频ID")]
    public override string Id { get; set; }

    [SugarColumn(ColumnDescription = "视频标题")]
    public string Title { get; set; }

    [SugarColumn(ColumnDescription = "主题 Key")]
    public string ThemeKey { get; set; }

    [SugarColumn(ColumnDescription = "视频文件地址")]
    public string FileUrl { get; set; }

    [SugarColumn(ColumnDescription = "封面地址")]
    public string CoverUrl { get; set; }

    [SugarColumn(ColumnDescription = "时长（秒）")]
    public int DurationSec { get; set; }

    [SugarColumn(ColumnDescription = "状态：uploaded/analyzing/done/failed")]
    public string Status { get; set; } // uploaded/analyzing/done/failed

    [SugarColumn(ColumnDescription = "分析作业ID")]
    public long? AnalyzeJobId { get; set; }

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreateTime { get; set; }

    [SugarColumn(ColumnDescription = "更新时间")]
    public DateTime UpdateTime { get; set; }
}
