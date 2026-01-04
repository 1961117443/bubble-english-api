using Microsoft.AspNetCore.Http;
using QT.CMS.Interfaces;
using QT.Common.Const;
using QT.DependencyInjection;
using QT.Systems.Entitys.Permission;
using System.Security.Claims;

namespace QT.CMS;

/// <summary>
/// Identity用户接口实现
/// </summary>
public class UserService : IUserService,IScoped
{
    private readonly ISqlSugarRepository<UserEntity> _rep;
    private readonly IHttpContextAccessor _httpContextAccessor;
    //private readonly UserManager<ApplicationUser> _userManager;
    //private readonly RoleManager<ApplicationRole> _roleManager;

    private UserEntity _userEntity;

    public UserService(ISqlSugarRepository<UserEntity> rep,
        //UserManager<ApplicationUser> userManager,
        //RoleManager<ApplicationRole> roleManager,
        IHttpContextAccessor httpContextAccessor)  
    {
        _rep = rep;
        _httpContextAccessor = httpContextAccessor;
        //_userManager = userManager;
        //_roleManager = roleManager;
    }

    /// <summary>
    /// 判断当前登录用户是否超管
    /// </summary>
    public async Task<bool> IsSuperAdminAsync()
    {
        var u = await GetUserAsync();
        if (u!=null)
        {
            return u.IsAdministrator == 1;
        }
        ////获得当前用户组
        //List<Claim> claimsList = _httpContextAccessor.HttpContext.User.Claims.Where(t => t.Type == ClaimTypes.Role).ToList();
        //if (claimsList == null)
        //{
        //    return false;
        //}
        ////遍历用户所拥有的角色
        //foreach (var claim in claimsList)
        //{
        //    ApplicationRole role = await _roleManager.FindByNameAsync(claim.Value);
        //    if (role == null)
        //    {
        //        continue;
        //    }
        //    //如果是超级管理员则直接允许访问
        //    if (role.RoleType == (int)RoleType.SuperAdmin)
        //    {
        //        return true;
        //    }
        //}
        return false;
    }

    /// <summary>
    /// 获取当前登录用户ID
    /// </summary>
    public async Task<string> GetUserIdAsync()
    {
        if (_httpContextAccessor.HttpContext.User.FindFirst(ClaimConst.CLAINMUSERID) == null)
        {
            return string.Empty;
        }
        var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
        var isExist = await _rep.AnyAsync(x => x.Id == userId);
        if (isExist)
        {
            return userId;
        }
        else
        {
            return string.Empty;
        }
        //    var model = await _userManager.FindByIdAsync(userId);
        //if (model != null)
        //{
        //    return model.Id;
        //}
        //return 0;
    }

    /// <summary>
    /// 获取当前登录用户名
    /// </summary>
    public async Task<string?> GetUserNameAsync()
    {
        if (_httpContextAccessor.HttpContext.User.FindFirst(ClaimConst.CLAINMUSERID) == null)
        {
            return null;
        }

        var model = await GetUserAsync();

        //var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //var model = await _userManager.FindByIdAsync(userId);
        if (model != null)
        {
            return model.RealName;
        }
        return null;
    }

    /// <summary>
    /// 获取当前登录用户信息
    /// </summary>
    public async Task<UserEntity?> GetUserAsync()
    {
        if (_httpContextAccessor.HttpContext.User.FindFirst(ClaimConst.CLAINMUSERID) == null)
        {
            return null;
        }
        var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
        if (_userEntity!=null && _userEntity.Id == userId)
        {
            return _userEntity;
        }
        else
        {
            _userEntity = await _rep.SingleAsync(x => x.Id == userId);
            return _userEntity;
        }
    }

    ///// <summary>
    ///// 获取当前登录用户角色所有Claims
    ///// </summary>
    //public async Task<List<Claim>> GetRoleClaimsAsync()
    //{
    //    List<Claim> claimList = new List<Claim>();
    //    //获得当前用户组
    //    List<Claim> claimRoleList = _httpContextAccessor.HttpContext.User.Claims.Where(t => t.Type == ClaimTypes.Role).ToList();
    //    if (claimRoleList == null)
    //    {
    //        return claimList;
    //    }
    //    //遍历用户所拥有的角色
    //    foreach (var claimRole in claimRoleList)
    //    {
    //        ApplicationRole role = await _roleManager.FindByNameAsync(claimRole.Value);
    //        if (role == null)
    //        {
    //            continue;
    //        }
    //        //验证角色的权限是否一致
    //        IList<Claim> roleClaims = await _roleManager.GetClaimsAsync(role);
    //        if (roleClaims == null)
    //        {
    //            continue;
    //        }
    //        foreach (var claim in roleClaims)
    //        {
    //            if (claimList.Any(x => x.Type == claim.Type && x.Value == claim.Value))
    //            {
    //                continue;
    //            }
    //            claimList.Add(claim);
    //        }
    //    }
    //    return claimList;
    //}

    ///// <summary>
    ///// 获取指定角色所有Claims
    ///// </summary>
    //public async Task<List<Claim>> GetRoleClaimsAsync(int roleId)
    //{
    //    List<Claim> claimList = new List<Claim>();
    //    //获取角色的Claims
    //    ApplicationRole role = await _roleManager.FindByIdAsync(roleId.ToString());
    //    if (role == null)
    //    {
    //        return claimList;
    //    }
    //    IList<Claim> roleClaims = await _roleManager.GetClaimsAsync(role);
    //    if (roleClaims == null)
    //    {
    //        return claimList;
    //    }
    //    foreach (var claim in roleClaims)
    //    {
    //        if (claimList.Any(x => x.Type == claim.Type && x.Value == claim.Value))
    //        {
    //            continue;
    //        }
    //        claimList.Add(claim);
    //    }
    //    return claimList;
    //}

    ///// <summary>
    ///// 修改当前用户密码
    ///// </summary>
    //public async Task<bool> UpdatePasswordAsync(PasswordDto modelDto)
    //{
    //    var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //    var model =await GetUserAsync(); // await _userManager.FindByIdAsync(userId);
    //    if (model == null)
    //    {
    //        throw Oops.Oh("尚未登录或已超时，请登录后操作");
    //    }
    //    if (modelDto.NewPassword != modelDto.ConfirmPassword)
    //    {
    //        throw Oops.Oh("两次输入的密码不一至，请重试");
    //    }
    //    if(!await _userManager.CheckPasswordAsync(model, modelDto.Password))
    //    {
    //        throw Oops.Oh("旧密码不正确，请重试");
    //    }

    //    var result = await _userManager.ChangePasswordAsync(model, modelDto.Password, modelDto.NewPassword);
    //    if (!result.Succeeded)
    //    {
    //        throw Oops.Oh($"错误代码：{result.Errors.FirstOrDefault()?.Code}");
    //    }
    //    return true;
    //}

}