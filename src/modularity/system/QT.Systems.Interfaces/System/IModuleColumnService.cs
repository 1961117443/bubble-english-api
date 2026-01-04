using QT.Systems.Entitys.Dto.ModuleColumn;
using QT.Systems.Entitys.System;

namespace QT.Systems.Interfaces.System;

/// <summary>
/// 功能列表



/// 日 期：2021-06-01.
/// </summary>
public interface IModuleColumnService
{
    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="moduleId">功能id.</param>
    /// <returns></returns>
    Task<List<ModuleColumnEntity>> GetList(string? moduleId = default);

    /// <summary>
    /// 获取用户功能列表.
    /// </summary>
    Task<dynamic> GetUserModuleColumnList();
}