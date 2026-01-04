using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.WorkOrderLog;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class WorkOrderLogInfoOutput: WorkOrderLogCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
