using QT.WorkFlow.Entitys.Dto.FlowBefore;
using QT.WorkFlow.Entitys.Dto.FlowTask;
using QT.WorkFlow.Entitys.Entity;
using QT.WorkFlow.Entitys.Model;

namespace QT.WorkFlow.Interfaces.Manager;

public interface IFlowTaskManager
{
    /// <summary>
    /// 获取任务详情.
    /// </summary>
    /// <param name="id">任务id.</param>
    /// <param name="taskNodeId">节点id.</param>
    /// <param name="taskOperatorId">经办id.</param>
    /// <returns></returns>
    Task<FlowBeforeInfoOutput> GetFlowBeforeInfo(string id, string taskNodeId, string taskOperatorId = null);

    /// <summary>
    /// 详情查看表单数据根据控件转换.
    /// </summary>
    /// <param name="entity">任务实例.</param>
    /// <returns></returns>
    Task<FlowTaskInfoOutput> GetFlowDynamicDataManage(FlowTaskEntity entity);

    /// <summary>
    /// 保存.
    /// </summary>
    /// <param name="id">任务主键id（通过空值判断是修改还是新增）.</param>
    /// <param name="flowId">引擎id.</param>
    /// <param name="processId">关联id.</param>
    /// <param name="flowTitle">任务名.</param>
    /// <param name="flowUrgent">紧急程度（自定义默认为1）.</param>
    /// <param name="billNo">单据规则.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="status">状态 1:保存，0提交.</param>
    /// <param name="approvaUpType">审批修改权限1：可写，0：可读.</param>
    /// <param name="isSysTable">true：系统表单，false：自定义表单.</param>
    /// <param name="parentId">任务父id.</param>
    /// <param name="crUser">子流程发起人.</param>
    /// <param name="isDev">是否功能设计.</param>
    /// <param name="isAsync">是否异步.</param>
    /// <returns></returns>
    Task<FlowTaskEntity> Save(
        string id,
        string flowId,
        string processId,
        string flowTitle,
        int? flowUrgent,
        string billNo,
        object formData,
        int status,
        int? approvaUpType = 0,
        bool isSysTable = true,
        string parentId = "0",
        string? crUser = null,
        bool isDev = false,
        bool isAsync = false);

    /// <summary>
    /// 提交.
    /// </summary>
    /// <param name="id">主键id（通过空值判断是修改还是新增）.</param>
    /// <param name="flowId">引擎id.</param>
    /// <param name="processId">关联id.</param>
    /// <param name="flowTitle">任务名.</param>
    /// <param name="flowUrgent">紧急程度（自定义默认为1）.</param>
    /// <param name="billNo">单据规则.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="status">状态 1:保存，0提交.</param>
    /// <param name="approvaUpType">审批修改权限1：可写，0：可读.</param>
    /// <param name="isSysTable">true：系统表单，false：自定义表单.</param>
    /// <param name="isDev">是否功能设计.</param>
    /// <param name="candidateList">候选人.</param>
    /// <returns></returns>
    Task<bool> Submit(string id, string flowId, string processId, string flowTitle, int? flowUrgent, string billNo, object formData, int status, int? approvaUpType = 0, bool isSysTable = true, bool isDev = false, Dictionary<string, List<string>> candidateList = null);

    /// <summary>
    /// 审批(同意).
    /// </summary>
    /// <param name="flowTaskEntity">任务实例.</param>
    /// <param name="flowTaskOperatorEntity">经办实例.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <param name="formType">表单类型.</param>
    /// <returns></returns>
    Task Audit(FlowTaskEntity flowTaskEntity, FlowTaskOperatorEntity flowTaskOperatorEntity, FlowHandleModel flowHandleModel, int formType);

    /// <summary>
    /// 审批(拒绝).
    /// </summary>
    /// <param name="flowTaskEntity">任务实例.</param>
    /// <param name="flowTaskOperatorEntity">经办实例.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <param name="formType">表单类型.</param>
    /// <returns></returns>
    Task Reject(FlowTaskEntity flowTaskEntity, FlowTaskOperatorEntity flowTaskOperatorEntity, FlowHandleModel flowHandleModel, int formType);

    /// <summary>
    /// 审批(撤回).
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowHandleModel">撤回参数.</param>
    Task Recall(string id, FlowHandleModel flowHandleModel);

    /// <summary>
    /// 流程撤回.
    /// </summary>
    /// <param name="flowTaskEntity">流程实例.</param>
    /// <param name="flowHandleModel">流程经办.</param>
    Task Revoke(FlowTaskEntity flowTaskEntity, string flowHandleModel);

    /// <summary>
    /// 终止.
    /// </summary>
    /// <param name="flowTaskEntity">流程实例.</param>
    /// <param name="flowHandleModel">流程经办.</param>
    Task Cancel(FlowTaskEntity flowTaskEntity, FlowHandleModel flowHandleModel);

    /// <summary>
    /// 指派.
    /// </summary>
    /// <param name="id">任务id.</param>
    /// <param name="flowHandleModel">指派参数.</param>
    /// <returns></returns>
    Task Assigned(string id, FlowHandleModel flowHandleModel);

    /// <summary>
    /// 转办.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowHandleModel">转办参数.</param>
    /// <returns></returns>
    Task Transfer(string id, FlowHandleModel flowHandleModel);

    /// <summary>
    /// 催办.
    /// </summary>
    /// <param name="id">任务id.</param>
    /// <returns></returns>
    Task Press(string id);

    /// <summary>
    /// 审批事前操作.
    /// </summary>
    /// <param name="flowEngineEntity">流程实例.</param>
    /// <param name="flowTaskEntity">任务实例.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <returns></returns>
    Task ApproveBefore(FlowEngineEntity flowEngineEntity, FlowTaskEntity flowTaskEntity, FlowHandleModel flowHandleModel);

    /// <summary>
    /// 获取候选人.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <param name="type">0:候选节点编码，1：候选人.</param>
    /// <returns></returns>
    Task<dynamic> GetCandidateModelList(string id, FlowHandleModel flowHandleModel, int type = 0);

    /// <summary>
    /// 批量审批节点列表.
    /// </summary>
    /// <param name="flowId">流程id.</param>
    /// <returns></returns>
    Task<dynamic> NodeSelector(string flowId);

    /// <summary>
    /// 获取批量审批候选人.
    /// </summary>
    /// <param name="flowId">流程id.</param>
    /// <param name="flowTaskOperatorId">经办id.</param>
    /// <returns></returns>
    Task<dynamic> GetBatchCandidate(string flowId, string flowTaskOperatorId);

    /// <summary>
    /// 审批根据条件变更节点.
    /// </summary>
    /// <param name="flowEngineEntity">流程实例.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="flowTaskOperatorEntity">经办实例.</param>
    /// <returns></returns>
    Task AdjustNodeByCon(FlowEngineEntity flowEngineEntity, object formData, FlowTaskOperatorEntity flowTaskOperatorEntity);

    /// <summary>
    /// 判断驳回节点是否存在子流程.
    /// </summary>
    /// <param name="flowTaskOperatorEntity">经办实例.</param>
    /// <returns></returns>
    Task<bool> IsSubFlowUpNode(FlowTaskOperatorEntity flowTaskOperatorEntity);

    /// <summary>
    /// 获取批量任务的表单数据.
    /// </summary>
    /// <param name="taskOperatorId">经办id.</param>
    /// <returns></returns>
    Task<object> GetBatchOperationData(string taskOperatorId);

    /// <summary>
    /// 详情操作验证.
    /// </summary>
    /// <param name="taskOperatorId">经办id.</param>
    /// <returns></returns>
    Task Validation(string taskOperatorId);
}
