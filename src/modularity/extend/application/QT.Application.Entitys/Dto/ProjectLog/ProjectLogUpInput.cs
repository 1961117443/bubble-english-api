using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.ProjectLog;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class ProjectLogUpInput : ProjectLogCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
