using QT.Systems.Entitys.System;

namespace QT.Systems.Interfaces.System;

/// <summary>
/// 数据权限方案



/// 日 期：2021-06-01.
/// </summary>
public interface IModuleDataAuthorizeSchemeService
{
    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="moduleId">功能id.</param>
    /// <returns></returns>
    Task<List<ModuleDataAuthorizeSchemeEntity>> GetList(string? moduleId = default);

    /// <summary>
    /// 获取用户数据权限方案.
    /// </summary>
    Task<dynamic> GetResourceList();
}