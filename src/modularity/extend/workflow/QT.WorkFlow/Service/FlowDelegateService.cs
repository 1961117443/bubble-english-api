using System.Text.Json;
using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.WorkFlow.Entitys.Dto.FlowDelegete;
using QT.WorkFlow.Entitys.Entity;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.WorkFlow.Service;

/// <summary>
/// 流程委托.
/// </summary>
[ApiDescriptionSettings(Tag = "WorkflowEngine", Name = "FlowDelegate", Order = 300)]
[Route("api/workflow/Engine/[controller]")]
public class FlowDelegateService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<FlowDelegateEntity> _flowDelegateRepository;
    private readonly IUserManager _userManager;

    public FlowDelegateService(ISqlSugarRepository<FlowDelegateEntity> flowDelegateRepository, IUserManager userManager)
    {
        _flowDelegateRepository = flowDelegateRepository;
        _userManager = userManager;
    }

    #region GET

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] PageInputBase input)
    {
        var list = await _flowDelegateRepository.AsQueryable().Where(x => x.CreatorUserId == _userManager.UserId && x.DeleteMark == null)
            .WhereIF(!input.keyword.IsNullOrEmpty(), m => m.FlowName.Contains(input.keyword) || m.FlowCategory.Contains(input.keyword)).OrderBy(t => t.SortCode)
            .OrderBy(x => x.CreatorTime, OrderByType.Desc).OrderByIF(!string.IsNullOrEmpty(input.keyword), t => t.LastModifyTime, OrderByType.Desc).ToPagedListAsync(input.currentPage, input.pageSize);
        var pageList = new SqlSugarPagedList<FlowDelegeteListOutput>()
        {
            list = list.list.Adapt<List<FlowDelegeteListOutput>>(),
            pagination = list.pagination
        };
        return PageResult<FlowDelegeteListOutput>.SqlSugarPageResult(pageList);
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo_Api(string id)
    {
        return (await _flowDelegateRepository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null)).Adapt<FlowDelegeteInfoOutput>();
    }
    #endregion

    #region POST

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键值.</param>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _flowDelegateRepository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        var isOk = await _flowDelegateRepository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="jd">新建参数.</param>
    [HttpPost("")]
    public async Task Create([FromBody] FlowDelegeteCrInput input)
    {
        if (_userManager.UserId.Equals(input.toUserId))
            throw Oops.Oh(ErrorCode.WF0001);
        if (await _flowDelegateRepository.AnyAsync(x => x.FlowId == input.flowId && x.CreatorUserId == _userManager.UserId && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        input.endTime = input.endTime.ParseToDateTime().AddHours(23).AddMinutes(59);
        var entity = input.Adapt<FlowDelegateEntity>();
        var isOk = await _flowDelegateRepository.Context.Insertable(entity).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="jd">修改参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] FlowDelegeteUpInput input)
    {
        if (_userManager.UserId.Equals(input.toUserId))
            throw Oops.Oh(ErrorCode.WF0001);
        if (await _flowDelegateRepository.AnyAsync(x => x.Id != id && x.FlowId == input.flowId && x.CreatorUserId == _userManager.UserId && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        input.endTime = input.endTime.ParseToDateTime().AddHours(23).AddMinutes(59);

        var entity = input.Adapt<FlowDelegateEntity>();
        var isOk = await _flowDelegateRepository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }
    #endregion
}
