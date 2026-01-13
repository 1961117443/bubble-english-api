using QT.Common.Filter;
using System.ComponentModel.DataAnnotations;

namespace AI.BubbleEnglish.Dto;

public class AdminVideoQuery : PageInputBase
{
    public string? keyword { get; set; }
    public string? themeKey { get; set; }
    public string? status { get; set; }
}

public class AdminVideoCreateInput
{
    [Required(ErrorMessage = "title is required")]
    public string title { get; set; } = string.Empty;
    public string themeKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "fileUrl is required")]
    public string fileUrl { get; set; } = string.Empty;
    public string coverUrl { get; set; } = string.Empty;
    public int durationSec { get; set; } = 0;
}

public class AdminVideoUpdateInput : AdminVideoCreateInput
{
    public long id { get; set; }
    public string? status { get; set; }
}

public class AdminVideoOutput
{
    public string id { get; set; }
    public string title { get; set; } = string.Empty;
    public string themeKey { get; set; } = string.Empty;
    public string fileUrl { get; set; } = string.Empty;
    public string coverUrl { get; set; } = string.Empty;
    public int durationSec { get; set; }
    public string status { get; set; } = string.Empty;
    public long? analyzeJobId { get; set; }
    public DateTime createTime { get; set; }
    public DateTime updateTime { get; set; }
}

public class AdminAiAnalyzeInput
{
    public string videoId { get; set; }
    public string? provider { get; set; }
    public string? model { get; set; }
    public string? prompt { get; set; }
}
