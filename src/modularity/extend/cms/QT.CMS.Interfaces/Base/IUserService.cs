using QT.CMS.Entitys.Dto.Base;
using QT.Systems.Entitys.Permission;
using System.Security.Claims;

namespace QT.CMS.Interfaces;

/// <summary>
/// Identity用户接口
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 判断当前登录用户是否超管
    /// </summary>
    Task<bool> IsSuperAdminAsync();

    /// <summary>
    /// 获取当前登录用户ID
    /// </summary>
    Task<string> GetUserIdAsync();

    /// <summary>
    /// 获取当前登录用户名
    /// </summary>
    Task<string?> GetUserNameAsync();

    /// <summary>
    /// 获取当前登录用户信息
    /// </summary>
    Task<UserEntity?> GetUserAsync();

    ///// <summary>
    ///// 获取当前登录用户角色所有Claims
    ///// </summary>
    //Task<List<Claim>> GetRoleClaimsAsync();

    ///// <summary>
    ///// 获取指定角色所有Claims
    ///// </summary>
    //Task<List<Claim>> GetRoleClaimsAsync(int roleId);

    ///// <summary>
    ///// 修改用户密码
    ///// </summary>
    //Task<bool> UpdatePasswordAsync(PasswordDto modelDto);
}
