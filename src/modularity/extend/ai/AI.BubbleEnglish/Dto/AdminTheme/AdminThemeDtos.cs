using QT.Common.Filter;

namespace AI.BubbleEnglish.Dto;

/// <summary>
/// Admin: Theme
/// </summary>
public class AdminThemeQuery : PageInputBase
{
    public string? keyword { get; set; }
}

public class AdminThemeUpsertInput
{
    public string? id { get; set; }
    public string themeKey { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string coverUrl { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public int sort { get; set; } = 0;
    public int enabled { get; set; } = 1;
}

public class AdminThemeOutput
{
    public string id { get; set; } = string.Empty;
    public string themeKey { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string coverUrl { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public int sort { get; set; }
    public int enabled { get; set; }
    public DateTime createTime { get; set; }
}
