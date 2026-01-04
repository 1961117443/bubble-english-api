using QT.Systems.Entitys.System;

namespace QT.Systems.Interfaces.System;

/// <summary>
/// 表单权限



/// 日 期：2021-06-01.
/// </summary>
public interface IModuleFormService
{
    /// <summary>
    /// 表单权限列表.
    /// </summary>
    /// <param name="moduleId">功能id.</param>
    /// <returns></returns>
    Task<List<ModuleFormEntity>> GetList(string? moduleId = default);

    /// <summary>
    /// 获取用户功能表单.
    /// </summary>
    Task<dynamic> GetUserModuleFormList();
}