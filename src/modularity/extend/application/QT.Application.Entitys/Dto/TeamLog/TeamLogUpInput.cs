using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.TeamLog;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class TeamLogUpInput : TeamLogCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
