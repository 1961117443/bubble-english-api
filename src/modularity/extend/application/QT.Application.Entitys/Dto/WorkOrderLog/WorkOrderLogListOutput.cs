using QT.DependencyInjection;
using QT.Extend.Entitys.Dto.ProjectGantt;

namespace QT.Extend.Entitys.Dto.WorkOrderLog;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class WorkOrderLogListOutput: WorkOrderLogInfoOutput
{
    public DateTime? creatorTime { get; set; }
    public string creatorUserId { get; set; }

    public ManagersInfo managerInfo { get; set; }
}
