using QT.DependencyInjection;

namespace QT.WorkFlow.Entitys.Model;

[SuppressSniffer]
public class FlowTaskCandidateModel
{
    /// <summary>
    /// 节点编码.
    /// </summary>
    public string? nodeId { get; set; }

    /// <summary>
    /// 节点名.
    /// </summary>
    public string? nodeName { get; set; }
}
