using QT.DependencyInjection;

namespace QT.WorkFlow.Entitys.Dto.WorkFlowForm.LeaveApply;

[SuppressSniffer]
public class LeaveApplyUpInput : LeaveApplyCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }
}
