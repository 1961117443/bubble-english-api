using System.Text.Json;
using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.WorkFlow.Entitys.Dto.FlowBefore;
using QT.WorkFlow.Entitys.Enum;
using QT.WorkFlow.Entitys.Model;
using QT.WorkFlow.Interfaces.Manager;
using QT.WorkFlow.Interfaces.Repository;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SqlSugar;

namespace QT.WorkFlow.Service;

/// <summary>
/// 流程审批.
/// </summary>
[ApiDescriptionSettings(Tag = "WorkflowEngine", Name = "FlowBefore", Order = 303)]
[Route("api/workflow/Engine/[controller]")]
public class FlowBeforeService : IDynamicApiController, ITransient
{
    private readonly IFlowTaskRepository _flowTaskRepository;
    private readonly IFlowTaskManager _flowTaskManager;
    private readonly IUserManager _userManager;

    public FlowBeforeService(IFlowTaskRepository flowTaskRepository, IFlowTaskManager flowTaskManager, IUserManager userManager)
    {
        _flowTaskRepository = flowTaskRepository;
        _flowTaskManager = flowTaskManager;
        _userManager = userManager;
    }

    #region Get

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <param name="category">分类.</param>
    /// <returns></returns>
    [HttpGet("List/{category}")]
    public async Task<dynamic> GetList([FromQuery] FlowBeforeListQuery input, string category)
    {
        try
        {
            switch (category)
            {
                case "1":
                    return await _flowTaskRepository.GetWaitList(input);
                case "2":
                    return await _flowTaskRepository.GetTrialList(input);
                case "3":
                    return await _flowTaskRepository.GetCirculateList(input);
                case "4":
                    return await _flowTaskRepository.GetBatchWaitList(input);
                default:
                    return PageResult<FlowBeforeListOutput>.SqlSugarPageResult(new SqlSugarPagedList<FlowBeforeListOutput>());
            }
        }
        catch (Exception ex)
        {
            return PageResult<FlowBeforeListOutput>.SqlSugarPageResult(new SqlSugarPagedList<FlowBeforeListOutput>());
        }
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="taskNodeId"></param>
    /// <param name="taskOperatorId"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id, [FromQuery] string taskNodeId, [FromQuery] string taskOperatorId)
    {
        try
        {
            return await _flowTaskManager.GetFlowBeforeInfo(id, taskNodeId, taskOperatorId);
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ErrorCode.WF0033);
        }
    }

    /// <summary>
    /// 审批汇总.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="category">分类（1：部门，2：角色，3：岗位）.</param>
    /// <returns></returns>
    [HttpGet("RecordList/{id}")]
    public async Task<dynamic> GetRecordList(string id, [FromQuery] string category, [FromQuery] string type)
    {
        var recordList = await _flowTaskRepository.GetRecordListByCategory(id, category, type);
        var categoryId = recordList.Select(x => x.category).Distinct().ToList();
        var list = new List<FlowBeforeRecordListOutput>();
        foreach (var item in categoryId)
        {
            var categoryList = recordList.FindAll(x => x.category == item).ToList();
            var output = new FlowBeforeRecordListOutput();
            output.fullName = categoryList.FirstOrDefault()?.categoryName;
            output.list = categoryList.OrderByDescending(x => x.handleTime).ToList();
            list.Add(output);
        }

        return list;
    }

    /// <summary>
    /// 获取候选人编码.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <returns></returns>
    [HttpPost("Candidates/{id}")]
    public async Task<dynamic> Candidates(string id, [FromBody] FlowHandleModel flowHandleModel)
    {
        if (id != "0")
        {
            await _flowTaskManager.Validation(id);
            var flowTaskOperatorEntity = await _flowTaskRepository.GetTaskOperatorInfo(id);
            var flowTaskEntity = await _flowTaskRepository.GetTaskInfo(flowTaskOperatorEntity.TaskId);
            var flowEngine = await _flowTaskRepository.GetEngineInfo(flowTaskEntity.FlowId);
            await _flowTaskManager.AdjustNodeByCon(flowEngine, flowHandleModel.formData, flowTaskOperatorEntity);
        }

        return await _flowTaskManager.GetCandidateModelList(id, flowHandleModel);
    }

    /// <summary>
    /// 获取候选人.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <returns></returns>
    [HttpPost("CandidateUser/{id}")]
    public async Task<dynamic> CandidateUser(string id, [FromBody] FlowHandleModel flowHandleModel)
    {
        return await _flowTaskManager.GetCandidateModelList(id, flowHandleModel, 1);
    }

    /// <summary>
    /// 批量审批流程列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("BatchFlowSelector")]
    public async Task<dynamic> BatchFlowSelector()
    {
        return await _flowTaskRepository.BatchFlowSelector();
    }

    /// <summary>
    /// 批量审批节点列表.
    /// </summary>
    /// <param name="id">流程id.</param>
    /// <returns></returns>
    [HttpGet("NodeSelector/{id}")]
    public async Task<dynamic> NodeSelector(string id)
    {
        return await _flowTaskManager.NodeSelector(id);
    }

    /// <summary>
    /// 批量审批候选人.
    /// </summary>
    /// <param name="flowId">流程id.</param>
    /// <param name="taskOperatorId">经办id.</param>
    /// <returns></returns>
    [HttpGet("BatchCandidate")]
    public async Task<dynamic> GetBatchCandidate([FromQuery] string flowId, [FromQuery] string taskOperatorId)
    {
        await _flowTaskManager.Validation(taskOperatorId);
        return await _flowTaskManager.GetBatchCandidate(flowId, taskOperatorId);
    }

    /// <summary>
    /// 验证站内信详情是否有查看权限.
    /// </summary>
    /// <param name="taskOperatorId">经办id.</param>
    /// <returns></returns>
    [HttpGet("{taskOperatorId}/Info")]
    public async Task IsInfo(string taskOperatorId)
    {
        var flowTaskOperatorEntity = await _flowTaskRepository.GetTaskOperatorInfo(taskOperatorId);
        if (flowTaskOperatorEntity.IsNullOrEmpty())
            throw Oops.Oh(ErrorCode.WF0029);
        var flowTaskEntity = await _flowTaskRepository.GetTaskInfo(flowTaskOperatorEntity.TaskId);
        if (flowTaskOperatorEntity.HandleId == _userManager.UserId)
        {
            if (flowTaskOperatorEntity.Completion != 0 || flowTaskOperatorEntity.State == "-1" || flowTaskEntity.Status == 5)
                throw Oops.Oh(ErrorCode.WF0029);
        }
        else
        {
            var toUserId = _flowTaskRepository.GetToUserId(flowTaskOperatorEntity.HandleId, flowTaskEntity.FlowId);
            if (!toUserId.Contains(_userManager.UserId) || flowTaskOperatorEntity.Completion != 0 || flowTaskOperatorEntity.State == "-1" || flowTaskEntity.Status == 5)
                throw Oops.Oh(ErrorCode.WF0029);
        }
    }
    #endregion

    #region POST

    /// <summary>
    /// 审核同意.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <returns></returns>
    [HttpPost("Audit/{id}")]
    [SqlSugarUnitOfWork]
    public async Task Audit(string id, [FromBody] FlowHandleModel flowHandleModel)
    {
        await _flowTaskManager.Validation(id);
        var flowTaskOperatorEntity = await _flowTaskRepository.GetTaskOperatorInfo(id);
        var flowTaskEntity = await _flowTaskRepository.GetTaskInfo(flowTaskOperatorEntity.TaskId);
        var flowEngine = await _flowTaskRepository.GetEngineInfo(flowTaskEntity.FlowId);
        await _flowTaskManager.Audit(flowTaskEntity, flowTaskOperatorEntity, flowHandleModel, (int)flowEngine.FormType);
        await _flowTaskManager.ApproveBefore(flowEngine, flowTaskEntity, flowHandleModel);
    }

    /// <summary>
    /// 审核拒绝.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <returns></returns>
    [HttpPost("Reject/{id}")]
    public async Task Reject(string id, [FromBody] FlowHandleModel flowHandleModel)
    {
        await _flowTaskManager.Validation(id);
        var flowTaskOperatorEntity = await _flowTaskRepository.GetTaskOperatorInfo(id);
        var flowTaskEntity = await _flowTaskRepository.GetTaskInfo(flowTaskOperatorEntity.TaskId);
        var flowEngine = await _flowTaskRepository.GetEngineInfo(flowTaskEntity.FlowId);
        if (await _flowTaskRepository.AnyFlowTask(x => x.ParentId == flowTaskOperatorEntity.TaskId && x.Status == 0 && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.WF0019);
        if (await _flowTaskManager.IsSubFlowUpNode(flowTaskOperatorEntity))
            throw Oops.Oh(ErrorCode.WF0019);
        flowTaskEntity = await _flowTaskRepository.GetTaskInfo(flowTaskOperatorEntity.TaskId);
        await _flowTaskManager.Reject(flowTaskEntity, flowTaskOperatorEntity, flowHandleModel, (int)flowEngine.FormType);
    }

    /// <summary>
    /// 审批撤回.
    /// 注意：在撤销流程时要保证你的下一节点没有处理这条记录；如已处理则无法撤销流程.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <returns></returns>
    [HttpPost("Recall/{id}")]
    public async Task Recall(string id, [FromBody] FlowHandleModel flowHandleModel)
    {
        var flowTaskOperatorRecord = await _flowTaskRepository.GetTaskOperatorRecordInfo(id);
        if (await _flowTaskRepository.AnyFlowTask(x => x.ParentId == flowTaskOperatorRecord.TaskId && x.Status == 0 && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.WF0018);
        await _flowTaskManager.Recall(id, flowHandleModel);
    }

    /// <summary>
    /// 终止审核.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <returns></returns>
    [HttpPost("Cancel/{id}")]
    public async Task Cancel(string id, [FromBody] FlowHandleModel flowHandleModel)
    {
        if (await _flowTaskRepository.AnyFlowTask(x => x.ParentId == id && x.Status == 0 && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.WF0017);
        var flowTaskEntity = await _flowTaskRepository.GetTaskInfo(id);
        if (!flowTaskEntity.ParentId.Equals("0") && flowTaskEntity.ParentId.IsNotEmptyOrNull())
            throw Oops.Oh(ErrorCode.WF0015);
        if (flowTaskEntity.FlowType == 1)
            throw Oops.Oh(ErrorCode.WF0016);
        await _flowTaskManager.Cancel(flowTaskEntity, flowHandleModel);
    }

    /// <summary>
    /// 转办.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <returns></returns>
    [HttpPost("Transfer/{id}")]
    public async Task Transfer(string id, [FromBody] FlowHandleModel flowHandleModel)
    {
        await _flowTaskManager.Validation(id);
        await _flowTaskManager.Transfer(id, flowHandleModel);
    }

    /// <summary>
    /// 指派.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <returns></returns>
    [HttpPost("Assign/{id}")]
    public async Task Assigned(string id, [FromBody] FlowHandleModel flowHandleModel)
    {
        var nodeEntity = await _flowTaskRepository.GetTaskNodeList(x => x.TaskId == id && x.State.Equals("0") && FlowTaskNodeTypeEnum.subFlow.ParseToString().Equals(x.NodeType) && x.NodeCode.Equals(flowHandleModel.nodeCode));
        if (nodeEntity.IsNotEmptyOrNull() && nodeEntity.Count > 0)
            throw Oops.Oh(ErrorCode.WF0014);
        await _flowTaskManager.Assigned(id, flowHandleModel);
    }

    /// <summary>
    /// 保存审批草稿数据.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <returns></returns>
    [HttpPost("SaveAudit/{id}")]
    [SqlSugarUnitOfWork]
    public async Task SaveAudit(string id, [FromBody] FlowHandleModel flowHandleModel)
    {
        await _flowTaskManager.Validation(id);
        var flowTaskOperatorEntity = await _flowTaskRepository.GetTaskOperatorInfo(id);
        var flowTaskEntity = await _flowTaskRepository.GetTaskInfo(flowTaskOperatorEntity.TaskId);
        var flowEngine = await _flowTaskRepository.GetEngineInfo(flowTaskEntity.FlowId);
        flowTaskOperatorEntity.DraftData = flowEngine.FormType == 2 ? flowHandleModel.formData.ToObject<JObject>()["data"].ToString() : flowHandleModel.formData.ToJsonString();
        await _flowTaskRepository.UpdateTaskOperator(flowTaskOperatorEntity);
    }

    /// <summary>
    /// 批量审批.
    /// </summary>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <returns></returns>
    [HttpPost("BatchOperation")]
    public async Task BatchOperation([FromBody] FlowHandleModel flowHandleModel)
    {
        foreach (var item in flowHandleModel.ids)
        {
            flowHandleModel.formData = await _flowTaskManager.GetBatchOperationData(item);
            switch (flowHandleModel.batchType)
            {
                case 0:
                    var flowTaskOperatorEntity = await _flowTaskRepository.GetTaskOperatorInfo(item);
                    var flowTaskEntity = await _flowTaskRepository.GetTaskInfo(flowTaskOperatorEntity.TaskId);
                    var flowEngine = await _flowTaskRepository.GetEngineInfo(flowTaskEntity.FlowId);
                    if (flowTaskOperatorEntity == null)
                        throw Oops.Oh(ErrorCode.COM1005);
                    if (flowTaskOperatorEntity.Completion != 0)
                        throw Oops.Oh(ErrorCode.WF0006);
                    await _flowTaskManager.Audit(flowTaskEntity, flowTaskOperatorEntity, flowHandleModel, (int)flowEngine.FormType);
                    break;
                case 1:
                    await Reject(item, flowHandleModel);
                    break;
                case 2:
                    await Transfer(item, flowHandleModel);
                    break;
            }
        }
    }
    #endregion
}
