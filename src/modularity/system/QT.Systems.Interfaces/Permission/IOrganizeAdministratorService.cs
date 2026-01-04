using QT.Common.Models.User;

namespace QT.Systems.Interfaces.Permission;

/// <summary>
/// 分级管理
/// </summary>
public interface IOrganizeAdministratorService
{
    /// <summary>
    /// 获取用户数据范围.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<UserDataScopeModel>> GetUserDataScopeModel(string userId);
}