using QT.DependencyInjection;
using QT.Extend.Entitys.Dto.ProjectGantt;

namespace QT.Extend.Entitys.Dto.ProjectLog;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class ProjectLogListOutput: ProjectLogInfoOutput
{
    /// <summary>
    /// 发送人
    /// </summary>
    public string? sender { get; set; }

    public DateTime? sendTime { get; set; }
    public string creatorUserId { get; set; }


    /// <summary>
    /// 发送人信息
    /// </summary>
    public ManagersInfo? managerInfo { get; set; }
}
