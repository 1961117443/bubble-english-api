using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.WorkFlow.Entitys.Dto.FlowTask;
using QT.WorkFlow.Interfaces.Manager;
using QT.WorkFlow.Interfaces.Repository;
using Microsoft.AspNetCore.Mvc;

namespace QT.WorkFlow.Service;

/// <summary>
/// 流程任务.
/// </summary>
[ApiDescriptionSettings(Tag = "WorkflowEngine", Name = "FlowTask", Order = 306)]
[Route("api/workflow/Engine/[controller]")]
public class FlowTaskService : IDynamicApiController, ITransient
{
    private readonly IFlowTaskRepository _flowTaskRepository;
    private readonly IFlowTaskManager _flowTaskManager;
    private readonly IUserManager _userManager;

    public FlowTaskService(IFlowTaskRepository flowTaskRepository, IFlowTaskManager flowTaskManager, IUserManager userManager)
    {
        _flowTaskRepository = flowTaskRepository;
        _flowTaskManager = flowTaskManager;
        _userManager = userManager;
    }

    #region Get

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var flowEntity = await _flowTaskRepository.GetTaskInfo(id);
        return await _flowTaskManager.GetFlowDynamicDataManage(flowEntity);
    }

    #endregion

    #region Post

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] FlowTaskCrInput input)
    {
        try
        {
            if (input.status == 1)
            {
                await _flowTaskManager.Save(null, input.flowId, null, null, 1, null, input.data.ToObject(), 1, 0, false);
            }
            else
            {
                var flag = await _flowTaskManager.Submit(null, input.flowId, null, null, 1, null, input.data.ToObject(), 0, 0, false, false, input.candidateList);
                if (!flag)
                    throw Oops.Oh(ErrorCode.WF0005);
            }
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ErrorCode.WF0005);
        }
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] FlowTaskUpInput input)
    {
        try
        {
            //if (_userManager.UserId.Equals("admin"))
            //    throw QTException.Oh(ErrorCode.WF0004);
            if (input.status == 1)
            {
                await _flowTaskManager.Save(id, input.flowId, null, null, 1, null, input.data.ToObject(), 1, 0, false);
            }
            else
            {
                var flag = await _flowTaskManager.Submit(id, input.flowId, null, null, 1, null, input.data.ToObject(), 0, 0, false, false, input.candidateList);
                if (!flag)
                    throw Oops.Oh(ErrorCode.WF0005);
            }
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ErrorCode.WF0005);
        }
    }
    #endregion
}
