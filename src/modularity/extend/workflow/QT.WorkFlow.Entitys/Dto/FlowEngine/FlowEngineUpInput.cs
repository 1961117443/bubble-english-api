using QT.DependencyInjection;

namespace QT.WorkFlow.Entitys.Dto.FlowEngine;

[SuppressSniffer]
public class FlowEngineUpInput : FlowEngineCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
