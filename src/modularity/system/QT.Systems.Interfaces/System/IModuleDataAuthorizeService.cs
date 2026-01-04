using QT.Systems.Entitys.System;

namespace QT.Systems.Interfaces.System;

/// <summary>
/// 数据权限



/// 日 期：2021-06-01.
/// </summary>
public interface IModuleDataAuthorizeService
{
    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="moduleId">功能id.</param>
    /// <returns></returns>
    Task<List<ModuleDataAuthorizeEntity>> GetList(string? moduleId = default);
}