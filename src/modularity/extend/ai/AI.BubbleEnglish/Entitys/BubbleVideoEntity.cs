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

    [SugarColumn(ColumnDescription = "本地文件路径（服务器绝对路径）", IsNullable = true)]
    public string? LocalPath { get; set; }

    [SugarColumn(ColumnDescription = "工作目录（存放派生文件）", IsNullable = true)]
    public string? WorkDir { get; set; }

    [SugarColumn(ColumnDescription = "可供AI分析的源文本（字幕/ASR/人工）", IsNullable = true)]
    public string? SourceText { get; set; }

    [SugarColumn(ColumnDescription = "源文本类型：subtitle/asr/manual/none", IsNullable = true)]
    public string? SourceTextType { get; set; }

    [SugarColumn(ColumnDescription = "源文本语言：en/zh/mix", IsNullable = true)]
    public string? SourceTextLang { get; set; }

    [SugarColumn(ColumnDescription = "最近错误信息", IsNullable = true)]
    public string? LastError { get; set; }

    [SugarColumn(ColumnDescription = "分析作业ID")]
    public long? AnalyzeJobId { get; set; }

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreateTime { get; set; }

    [SugarColumn(ColumnDescription = "更新时间")]
    public DateTime UpdateTime { get; set; }
}
