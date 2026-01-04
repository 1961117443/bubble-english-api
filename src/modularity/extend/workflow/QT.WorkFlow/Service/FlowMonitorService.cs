using QT.Common.Enum;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.WorkFlow.Entitys.Dto.FlowMonitor;
using QT.WorkFlow.Interfaces.Repository;
using Microsoft.AspNetCore.Mvc;

namespace QT.WorkFlow.Service;

/// <summary>
/// 流程监控.
/// </summary>
[ApiDescriptionSettings(Tag = "WorkflowEngine", Name = "FlowMonitor", Order = 304)]
[Route("api/workflow/Engine/[controller]")]
public class FlowMonitorService : IDynamicApiController, ITransient
{
    private readonly IFlowTaskRepository _flowTaskRepository;

    /// <param name="flowTaskRepository"></param>
    public FlowMonitorService(IFlowTaskRepository flowTaskRepository)
    {
        _flowTaskRepository = flowTaskRepository;
    }

    #region GET

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] FlowMonitorListQuery input)
    {
        return await _flowTaskRepository.GetMonitorList(input);
    }

    /// <summary>
    /// 批量删除.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task Delete([FromBody] FlowMonitorDeleteInput input)
    {
        foreach (var item in input.ids.Split(","))
        {
            var entity=await _flowTaskRepository.GetTaskInfo(item);
            if (entity == null)
                throw Oops.Oh(ErrorCode.COM1005);
            if (!entity.ParentId.Equals("0") && entity.ParentId.IsNotEmptyOrNull())
                throw Oops.Oh(ErrorCode.WF0003);
            if (entity.FlowType == 1)
                throw Oops.Oh(ErrorCode.WF0012);
            await _flowTaskRepository.DeleteTask(entity);
        }
    }
    #endregion
}
