using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Systems.Entitys.Dto.DataInterfaceLog;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Systems;

/// <summary>
/// 数据接口日志



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "DataInterfaceLog", Order = 204)]
[Route("api/system/[controller]")]
public class DataInterfaceLogService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<DataInterfaceLogEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="DataInterfaceLogService"/>类型的新实例.
    /// </summary>
    public DataInterfaceLogService(
        ISqlSugarRepository<DataInterfaceLogEntity> dataInterfaceLogRepository,
        IUserManager userManager)
    {
        _repository = dataInterfaceLogRepository;
        _userManager = userManager;
    }

    #region Get

    [HttpGet("{id}")]
    public async Task<dynamic> GetList(string id, [FromQuery] PageInputBase input)
    {
        var list = await _repository.Context.Queryable<DataInterfaceLogEntity, UserEntity>((a, b) =>
        new JoinQueryInfos(JoinType.Left, b.Id == a.UserId))
             .Where(a => a.InvokId == id)
             .WhereIF(input.keyword.IsNotEmptyOrNull(), a => a.UserId.Contains(input.keyword) || a.InvokIp.Contains(input.keyword)).OrderBy(a => a.InvokTime)
            .Select((a, b) => new DataInterfaceLogListOutput
            {
                id = a.Id,
                invokDevice = a.InvokDevice,
                invokIp = a.InvokIp,
                userId = SqlFunc.MergeString(b.RealName, "/", b.Account),
                invokTime = a.InvokTime,
                invokType = a.InvokType,
                invokWasteTime = a.InvokWasteTime
            }).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<DataInterfaceLogListOutput>.SqlSugarPageResult(list);
    }
    #endregion
}