using QT.WorkFlow.Entitys.Dto.FlowEngine;

namespace QT.WorkFlow.Interfaces.Service;

/// <summary>
/// 流程设计.
/// </summary>
public interface IFlowEngineService
{
    /// <summary>
    /// 发起列表.
    /// </summary>
    /// <returns></returns>
    Task<List<FlowEngineListOutput>> GetFlowFormList();
}
