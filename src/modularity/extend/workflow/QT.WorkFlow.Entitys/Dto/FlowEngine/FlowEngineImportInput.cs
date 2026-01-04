using QT.DependencyInjection;
using QT.WorkFlow.Entitys.Entity;

namespace QT.WorkFlow.Entitys.Dto.FlowEngine;

[SuppressSniffer]
public class FlowEngineImportInput
{
    /// <summary>
    /// 导入流程.
    /// </summary>
    public FlowEngineEntity? flowEngine { get; set; }

    /// <summary>
    /// 导入流程权限.
    /// </summary>
    public List<FlowEngineVisibleEntity> visibleList { get; set; } = new List<FlowEngineVisibleEntity>();
}

