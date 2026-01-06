namespace AI.BubbleEnglish.Entitys;

using SqlSugar;
using System;

/// <summary>
/// AI 分析任务
/// </summary>
[SugarTable("bubble_ai_job", TableDescription = "Bubble AI 分析任务")]
public class BubbleAiJobEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "任务ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "视频ID")]
    public string VideoId { get; set; }

    [SugarColumn(ColumnDescription = "任务状态：queued/processing/success/failed")]
    public string Status { get; set; } // queued/processing/success/failed

    [SugarColumn(ColumnDescription = "使用模型")]
    public string Model { get; set; }

    [SugarColumn(ColumnDescription = "提示词/Prompt")]
    public string Prompt { get; set; }

    [SugarColumn(ColumnDescription = "输出 JSON")]
    public string OutputJson { get; set; }

    [SugarColumn(ColumnDescription = "错误信息")]
    public string ErrorMessage { get; set; }

    [SugarColumn(ColumnDescription = "开始时间")]
    public DateTime? StartedAt { get; set; }

    [SugarColumn(ColumnDescription = "结束时间")]
    public DateTime? FinishedAt { get; set; }

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreateTime { get; set; }
}
