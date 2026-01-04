using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.WorkFlow.Entitys.Dto.FlowEngine;

[SuppressSniffer]
public class FlowEngineListInput : PageInputBase
{
    /// <summary>
    /// 分类.
    /// </summary>
    public string? category { get; set; }
}

