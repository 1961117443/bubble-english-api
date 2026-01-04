using QT.Systems.Entitys.Dto.Module;
using QT.Systems.Entitys.System;

namespace QT.Systems.Interfaces.System;

/// <summary>
/// 菜单管理



/// 日 期：2021-06-01.
/// </summary>
public interface IModuleService
{
    /// <summary>
    /// 列表.
    /// </summary>
    /// <returns></returns>
    Task<List<ModuleEntity>> GetList();

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    Task<ModuleEntity> GetInfo(string id);

    /// <summary>
    /// 获取用户菜单树.
    /// </summary>
    /// <param name="type">登录类型.</param>
    /// <returns></returns>
    Task<List<ModuleNodeOutput>> GetUserTreeModuleList(string type);

    /// <summary>
    /// 获取用户菜单列表.
    /// </summary>
    /// <param name="type">登录类型.</param>
    Task<dynamic> GetUserModueList(string type);
}