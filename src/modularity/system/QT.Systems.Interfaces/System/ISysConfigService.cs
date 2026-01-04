using QT.Systems.Entitys.Dto.SysConfig;
using QT.Systems.Entitys.System;

namespace QT.Systems.Interfaces.System;

/// <summary>
/// 系统配置



/// 日 期：2021-06-01.
/// </summary>
public interface ISysConfigService
{
    /// <summary>
    /// 系统配置信息.
    /// </summary>
    /// <param name="category">分类</param>
    /// <param name="key">键</param>
    /// <returns></returns>
    Task<SysConfigEntity> GetInfo(string category, string key);

    /// <summary>
    /// 获取系统配置.
    /// </summary>
    /// <returns></returns>
    Task<SysConfigOutput> GetInfo();
}