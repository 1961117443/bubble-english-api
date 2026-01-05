namespace AI.BubbleEnglish.Dto;

/// <summary>
/// 后台：视频
/// </summary>
public class AdminVideoQuery
{
    public string? keyword { get; set; }
    public string? themeKey { get; set; }
    public string? status { get; set; }
}

public class AdminVideoCreateInput
{
    public string title { get; set; } = string.Empty;
    public string themeKey { get; set; } = string.Empty;
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
    public long id { get; set; }
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

/// <summary>
/// 后台：AI Job
/// </summary>
public class AdminAiJobOutput
{
    public long id { get; set; }
    public long videoId { get; set; }
    public string status { get; set; } = string.Empty;
    public string model { get; set; } = string.Empty;
    public string prompt { get; set; } = string.Empty;
    public string outputJson { get; set; } = string.Empty;
    public string errorMessage { get; set; } = string.Empty;
    public DateTime? startedAt { get; set; }
    public DateTime? finishedAt { get; set; }
    public DateTime createTime { get; set; }
}

public class AdminAiAnalyzeInput
{
    public long videoId { get; set; }
    public string? model { get; set; }
    public string? prompt { get; set; }
}

/// <summary>
/// 后台：Unit
/// </summary>
public class AdminUnitQuery
{
    public long? videoId { get; set; }
    public string? unitType { get; set; }
    public string? status { get; set; }
    public string? keyword { get; set; }
}

public class AdminUnitUpsertInput
{
    public long? id { get; set; }
    public long? videoId { get; set; }
    public string unitType { get; set; } = "word";
    public string text { get; set; } = string.Empty;
    public string meaning { get; set; } = string.Empty;
    public int minAge { get; set; } = 3;
    public string imageUrl { get; set; } = string.Empty;
    public string audioUrl { get; set; } = string.Empty;
    public string status { get; set; } = "draft";
}

public class AdminUnitOutput
{
    public long id { get; set; }
    public long? videoId { get; set; }
    public string unitType { get; set; } = string.Empty;
    public string text { get; set; } = string.Empty;
    public string meaning { get; set; } = string.Empty;
    public int minAge { get; set; }
    public string imageUrl { get; set; } = string.Empty;
    public string audioUrl { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;
    public DateTime createTime { get; set; }
    public DateTime updateTime { get; set; }
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
