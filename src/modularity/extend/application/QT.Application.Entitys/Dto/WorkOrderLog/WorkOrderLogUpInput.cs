using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.WorkOrderLog;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class WorkOrderLogUpInput : WorkOrderLogCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
