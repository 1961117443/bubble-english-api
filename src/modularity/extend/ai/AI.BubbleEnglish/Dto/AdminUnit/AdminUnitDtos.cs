using QT.Common.Filter;

namespace AI.BubbleEnglish.Dto;

/// <summary>
/// Admin: Unit
/// </summary>
public class AdminUnitQuery : PageInputBase
{
    public string? videoId { get; set; }
    public string? unitType { get; set; }
    public string? status { get; set; }
    public string? keyword { get; set; }
}

public class AdminUnitUpsertInput
{
    public string? id { get; set; }
    public string? videoId { get; set; }
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
    public string id { get; set; } = string.Empty;
    public string? videoId { get; set; }
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
