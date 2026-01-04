using QT.Systems.Entitys.System;

namespace QT.Systems.Interfaces.System;

/// <summary>
/// 功能按钮



/// 日 期：2021-06-01.
/// </summary>
public interface IModuleButtonService
{
    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="moduleId">功能id.</param>
    /// <returns></returns>
    Task<List<ModuleButtonEntity>> GetList(string? moduleId = default);

    /// <summary>
    /// 添加按钮.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<int> Create(ModuleButtonEntity entity);

    /// <summary>
    /// 获取用户功能按钮.
    /// </summary>
    Task<dynamic> GetUserModuleButtonList();
}