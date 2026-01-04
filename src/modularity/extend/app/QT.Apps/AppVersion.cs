using QT.Common.Configuration;
using QT.Common.Core.Manager;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Systems.Entitys.System;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Apps;

/// <summary>
/// App版本信息
/// </summary>
[ApiDescriptionSettings(Tag = "App", Name = "Version", Order = 806)]
[Route("api/App/[controller]")]
public class AppVersion : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<SysConfigEntity> _repository; // 系统设置

    /// <summary>
    /// 原始数据库.
    /// </summary>
    private readonly SqlSugarClient _db;

    /// <summary>
    /// 构造.
    /// </summary>
    /// <param name="sysConfigRepository"></param>
    /// <param name="context"></param>
    public AppVersion(
        ISqlSugarRepository<SysConfigEntity> sysConfigRepository,
        ISqlSugarClient context)
    {
        _repository = sysConfigRepository;
        _db = (SqlSugarClient)context;
    }

    #region Get

    /// <summary>
    /// 版本信息.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetInfo()
    {
        SysConfigEntity? data = new SysConfigEntity();

        if (KeyVariable.MultiTenancy)
        {
            data = await _db.Queryable<SysConfigEntity>().Where(x => x.Category.Equals("SysConfig") && x.Key == "sysVersion").FirstAsync();
        }
        else
        {
            data = await _repository.AsQueryable().Where(x => x.Category.Equals("SysConfig") && x.Key == "sysVersion").FirstAsync();
        }

        return new { sysVersion = data.Value };
    }

    #endregion
}