using QT.Systems.Entitys.System;

namespace QT.Apps.Interfaces;

/// <summary>
/// App常用数据
/// </summary>
public interface IAppDataService
{
    /// <summary>
    /// 菜单列表.
    /// </summary>
    /// <returns></returns>
    Task<List<ModuleEntity>> GetAppMenuList(string keyword);

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="objectId"></param>
    /// <returns></returns>
    Task Delete(string objectId);
}