namespace AI.BubbleEnglish.Dto;

/// <summary>
/// Admin: AI Job
/// </summary>
public class AdminAiJobOutput
{
    public string id { get; set; }
    public string videoId { get; set; }
    public string status { get; set; } = string.Empty;
    public string model { get; set; } = string.Empty;
    public string prompt { get; set; } = string.Empty;
    public string outputJson { get; set; } = string.Empty;
    public string errorMessage { get; set; } = string.Empty;
    public DateTime? startedAt { get; set; }
    public DateTime? finishedAt { get; set; }
    public DateTime createTime { get; set; }
}
