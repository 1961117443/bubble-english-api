using QT.Common.Enum;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.WorkFlow.Entitys.Dto.FlowLaunch;
using QT.WorkFlow.Interfaces.Manager;
using QT.WorkFlow.Interfaces.Repository;
using Microsoft.AspNetCore.Mvc;

namespace QT.WorkFlow.Service;

/// <summary>
/// 流程发起.
/// </summary>
[ApiDescriptionSettings(Tag = "WorkflowEngine", Name = "FlowLaunch", Order = 305)]
[Route("api/workflow/Engine/[controller]")]
public class FlowLaunchService : IDynamicApiController, ITransient
{
    private readonly IFlowTaskRepository _flowTaskRepository;
    private readonly IFlowTaskManager _flowTaskManager;

    public FlowLaunchService(IFlowTaskRepository flowTaskRepository, IFlowTaskManager flowTaskManager)
    {
        _flowTaskRepository = flowTaskRepository;
        _flowTaskManager = flowTaskManager;
    }

    #region GET

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] FlowLaunchListQuery input)
    {
        return await _flowTaskRepository.GetLaunchList(input);
    }
    #endregion

    #region POST

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _flowTaskRepository.GetTaskInfo(id);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        if (!entity.ParentId.Equals("0") && entity.ParentId.IsNotEmptyOrNull())
            throw Oops.Oh(ErrorCode.WF0003);
        if (entity.FlowType == 1)
            throw Oops.Oh(ErrorCode.WF0012);
        var isOk = await _flowTaskRepository.DeleteTask(entity);
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 撤回
    /// 注意：在撤回流程时要保证你的下一节点没有处理这条记录；如已处理则无法撤销流程.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">流程经办.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/Withdraw")]
    public async Task Revoke(string id, [FromBody] FlowLaunchActionWithdrawInput input)
    {
        var flowTaskEntity = await _flowTaskRepository.GetTaskInfo(id);
        var flowTaskNodeEntityList = await _flowTaskRepository.GetTaskNodeList(flowTaskEntity.Id);
        var flowTaskNodeEntity = flowTaskNodeEntityList.Find(m => m.SortCode == 2);
        await _flowTaskManager.Revoke(flowTaskEntity, input.handleOpinion);
    }

    /// <summary>
    /// 催办.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("Press/{id}")]
    public async Task Press(string id)
    {
        await _flowTaskManager.Press(id);
    }
    #endregion
}
