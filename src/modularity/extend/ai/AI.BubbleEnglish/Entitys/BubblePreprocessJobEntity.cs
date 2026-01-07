namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
using System;

/// <summary>
/// 视频预处理任务：字幕提取 / ASR 转写 / 音频切片等
/// </summary>
[SugarTable("bubble_preprocess_job", TableDescription = "Bubble 预处理任务")]
public class BubblePreprocessJobEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "任务ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "视频ID")]
    public string VideoId { get; set; } = default!;

    [SugarColumn(ColumnDescription = "任务类型：asr_transcribe/unit_audio")]
    public string Type { get; set; } = default!;

    [SugarColumn(ColumnDescription = "状态：queued/processing/success/failed")]
    public string Status { get; set; } = "queued";

    [SugarColumn(ColumnDescription = "结果文本（如 asr.txt）", IsNullable = true)]
    public string? ResultText { get; set; }

    [SugarColumn(ColumnDescription = "结果元数据JSON（如 srt 路径、segment等）", IsNullable = true)]
    public string? ResultMetaJson { get; set; }

    [SugarColumn(ColumnDescription = "错误信息", IsNullable = true)]
    public string? ErrorMessage { get; set; }

    [SugarColumn(ColumnDescription = "重试次数")]
    public int RetryCount { get; set; }

    [SugarColumn(ColumnDescription = "开始时间", IsNullable = true)]
    public DateTime? StartedAt { get; set; }

    [SugarColumn(ColumnDescription = "结束时间", IsNullable = true)]
    public DateTime? FinishedAt { get; set; }

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    [SugarColumn(ColumnDescription = "更新时间")]
    public DateTime UpdateTime { get; set; } = DateTime.Now;
}
