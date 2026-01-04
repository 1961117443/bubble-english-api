using QT.DependencyInjection;
using QT.Extend.Entitys.Dto.WorkLog;

namespace QT.Extend.Entitys.Dto.WoekLog;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class WorkLogUpInput : WorkLogCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
