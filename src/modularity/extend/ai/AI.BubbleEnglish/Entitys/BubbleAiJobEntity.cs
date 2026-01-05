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

    public long VideoId { get; set; }
    public string Status { get; set; } = "queued"; // queued/processing/success/failed
    public string Model { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string OutputJson { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;
}
