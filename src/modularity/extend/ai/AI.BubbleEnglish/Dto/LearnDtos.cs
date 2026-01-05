namespace AI.BubbleEnglish.Dto;

using QT.DependencyInjection;

[SuppressSniffer]
public class LearnReportInput
{
    public long childId { get; set; }
    public long? courseId { get; set; }
    public string? mode { get; set; } // guest/normal
    public int? durationMs { get; set; }

    // 用于统计的轻字段（前端可传，也可后端解析 reportJson 再算）
    public int? wordCount { get; set; }
    public int? sentenceCount { get; set; }

    /// <summary>原始上报 JSON（建议前端整包传）</summary>
    public string? reportJson { get; set; }
}

[SuppressSniffer]
public class LearnReportCreateOutput
{
    public long reportId { get; set; }
}

[SuppressSniffer]
public class StatsOutput
{
    public long childId { get; set; }
    public string type { get; set; } // daily/weekly/monthly
    public string key { get; set; }  // 2026-01-05 / 2026-01 / 2026-W01

    public int sessions { get; set; }
    public int learnDays { get; set; }
    public int words { get; set; }
    public int sentences { get; set; }
    public long durationMs { get; set; }
}

[SuppressSniffer]
public class EntitlementsOutput
{
    public bool isVip { get; set; }
    public List<string> themes { get; set; } = new();
}
