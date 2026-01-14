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


/// <summary>
/// 后台：Theme
/// </summary>
public class AdminThemeUpsertInput
{
    public long? id { get; set; }
    public string themeKey { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string coverUrl { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public int sort { get; set; } = 0;
    public int enabled { get; set; } = 1;
}

public class AdminThemeOutput
{
    public long id { get; set; }
    public string themeKey { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string coverUrl { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public int sort { get; set; }
    public int enabled { get; set; }
    public DateTime createTime { get; set; }
}


/// <summary>
/// 后台：Course
/// </summary>
public class AdminCourseQuery
{
    public string? keyword { get; set; }
    public string? themeKey { get; set; }
    public int? isPublish { get; set; }
}

public class AdminCourseUpsertInput
{
    public long? id { get; set; }
    public string title { get; set; } = string.Empty;
    public string cover { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public int sort { get; set; } = 0;
    public int isPublish { get; set; } = 0;
}

public class AdminCourseOutput
{
    public long id { get; set; }
    public string title { get; set; } = string.Empty;
    public string cover { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public int sort { get; set; }
    public int isPublish { get; set; }
    public DateTime createTime { get; set; }
}

