using QT.Common.Filter;

namespace AI.BubbleEnglish.Dto;

/// <summary>
/// Admin: Course
/// </summary>
public class AdminCourseQuery : PageInputBase
{
    public string? keyword { get; set; }
    public string? themeKey { get; set; }
    public int? isPublish { get; set; }
}

public class AdminCourseUpsertInput
{
    public string? id { get; set; }
    public string title { get; set; } = string.Empty;
    public string cover { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public int sort { get; set; } = 0;
    public int isPublish { get; set; } = 0;
}

public class AdminCourseOutput
{
    public string id { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string cover { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public int sort { get; set; }
    public int isPublish { get; set; }
    public DateTime createTime { get; set; }
}
