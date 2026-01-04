using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.DataSync;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using Microsoft.AspNetCore.Mvc;

namespace QT.Systems;

/// <summary>
/// 数据同步



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "DataSync", Order = 209)]
[Route("api/system/[controller]")]
public class DataSyncService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 数据连接服务.
    /// </summary>
    private readonly IDbLinkService _dbLinkService;

    /// <summary>
    /// 数据库管理.
    /// </summary>
    private readonly IDataBaseManager _dataBaseManager;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="DataSyncService"/>类型的新实例.
    /// </summary>
    public DataSyncService(
        IDataBaseManager dataBaseManager,
        IDbLinkService dbLinkService,
        IUserManager userManager)
    {
        _dataBaseManager = dataBaseManager;
        _dbLinkService = dbLinkService;
        _userManager = userManager;
    }

    /// <summary>
    /// 同步判断.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task<dynamic> Estimate([FromBody] DbSyncActionsExecuteInput input)
    {
        var linkFrom = await _dbLinkService.GetInfo(input.dbConnectionFrom);
        var linkTo = await _dbLinkService.GetInfo(input.dbConnectionTo)?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId,_userManager.TenantDbName);

        if (!IsNullDataByTable(linkFrom, input.dbTable))
        {
            // 初始表有数据
            return 1;
        }
        else if (!_dataBaseManager.IsAnyTable(linkTo, input.dbTable))
        {
            // 目的表不存在
            return 2;
        }
        else if (IsNullDataByTable(linkTo, input.dbTable))
        {
            // 目的表有数据
            return 3;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// 执行同步.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("Actions/Execute")]
    public async Task Execute([FromBody] DbSyncActionsExecuteInput input)
    {
        var linkFrom = await _dbLinkService.GetInfo(input.dbConnectionFrom);
        var linkTo = await _dbLinkService.GetInfo(input.dbConnectionTo) ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);

        _dataBaseManager.SyncTable(linkFrom, linkTo, input.dbTable, input.type);
        if (!await ImportTableData(linkFrom, linkTo, input.dbTable))
            throw Oops.Oh(ErrorCode.COM1006);
    }

    #region PrivateMethod

    /// <summary>
    /// 判断表中是否有数据
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="table"></param>
    /// <returns></returns>
    private bool IsNullDataByTable(DbLinkEntity entity, string table)
    {
        var data = _dataBaseManager.GetData(entity, table);
        if (data.Rows.Count > 0)
            return true;

        return false;
    }

    /// <summary>
    /// 批量写入.
    /// </summary>
    /// <param name="linkFrom">数据库连接 From.</param>
    /// <param name="linkTo">数据库连接To.</param>
    /// <param name="table"></param>
    private async Task<bool> ImportTableData(DbLinkEntity linkFrom, DbLinkEntity linkTo, string table)
    {
        try
        {
            // 取同步数据
            var syncData = _dataBaseManager.GetData(linkFrom, table);

            // 插入同步数据
            return await _dataBaseManager.SyncData(linkTo, syncData, table);
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    #endregion
}