using QT.DependencyInjection;

namespace QT.WorkFlow.Entitys.Dto.FlowLaunch;

[SuppressSniffer]
public class FlowLaunchActionWithdrawInput
{
    /// <summary>
    /// 撤回意见.
    /// </summary>
    public string? handleOpinion { get; set; }
}
