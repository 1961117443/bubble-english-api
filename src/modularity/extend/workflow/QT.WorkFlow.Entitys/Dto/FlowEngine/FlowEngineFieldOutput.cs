using QT.DependencyInjection;

namespace QT.WorkFlow.Entitys.Dto.FlowEngine;

[SuppressSniffer]
public class FlowEngineFieldOutput
{
    /// <summary>
    /// 字段.
    /// </summary>
    public string vmodel { get; set; }

    /// <summary>
    /// 字段说明.
    /// </summary>
    public string label { get; set; }
}
