using Mapster;
using Microsoft.AspNetCore.Http;
using QT.Common.Const;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.FriendlyException;
using QT.JZRC.Entitys;
using QT.JZRC.Entitys.Dto.AppService;
using QT.JZRC.Interfaces;
using SqlSugar;
using System.Security.Claims;

namespace QT.JZRC.Manager;

public class JzrcAppUserManager : IJzrcAppUserManager, IScoped
{
    public JzrcAppUserManager(IHttpContextAccessor httpContextAccessor, Func<string, ITransient, object> resolveNamed,ISqlSugarRepository<JzrcMemberEntity> repository)
    {
        _httpContextAccessor = httpContextAccessor;
        _resolveNamed = resolveNamed;
        _repository = repository;
    }

    //private AppLoginUser _appLoginUser;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Func<string, ITransient, object> _resolveNamed;
    private readonly ISqlSugarRepository<JzrcMemberEntity> _repository;

    //public AppLoginUser AppLoginUser
    //{
    //    get
    //    {
    //        if (_appLoginUser == null)
    //        {
    //            if (_httpContextAccessor.HttpContext !=null && _httpContextAccessor.HttpContext.User!=null)
    //            {
    //                var u = _httpContextAccessor.HttpContext.User;
    //                _appLoginUser = new AppLoginUser()
    //                {
    //                    Id = u.FindFirstValue(ClaimConst.CLAINMUSERID),
    //                    Account = u.FindFirstValue(ClaimConst.CLAINMACCOUNT),
    //                    RealName = u.FindFirstValue(ClaimConst.CLAINMREALNAME),
    //                    Role = u.FindFirstValue(nameof(AppLoginUserRole)).Adapt<AppLoginUserRole>()
    //                };
    //            }

    //        }
    //        return _appLoginUser;
    //    }
    //}

    public async Task<UserInfo> GetUserInfo()
    {
        var user = await _repository.AsQueryable().Where(x => x.Id == this.UserId)
            .Select(x => new JzrcMemberEntity
            {
                Id = x.Id,
                Account = x.Account,
                NickName = x.NickName,
                Role = x.Role,
                HeadIcon = x.HeadIcon,
            }).FirstAsync();
        //var loginService = _resolveNamed(this.Role.ToString(), default) as IJzrcAppLogin;
        //var user = await loginService.Login(loginDto) ?? throw Oops.Oh(ErrorCode.D5002);
        var u = new UserInfo
        {
            account = user.Account,
            id = user.Id,
            realName = user.NickName,
            role = user.Role,
            avatar = user.HeadIcon,
        };

        return u;
    }

    /// <summary>
    /// 当前租户id
    /// </summary>
    public string TenantId => _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimConst.TENANTID) ?? "";

    /// <summary>
    /// 账号
    /// </summary>
    public string Account => _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimConst.CLAINMACCOUNT) ?? "";


    /// <summary>
    /// 用户id
    /// </summary>
    public string UserId => _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimConst.CLAINMUSERID) ?? "";

    /// <summary>
    /// 用户昵称
    /// </summary>
    public string NickName => _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimConst.CLAINMREALNAME) ?? "";

    /// <summary>
    /// 用户角色
    /// </summary>
    public AppLoginUserRole Role => _httpContextAccessor!.HttpContext!.User.FindFirstValue(nameof(AppLoginUserRole)).Adapt<AppLoginUserRole>();


    /// <summary>
    /// 关联id
    /// </summary>
    public string RelationId => _httpContextAccessor?.HttpContext?.User.FindFirstValue(nameof(JzrcMemberEntity.RelationId)) ?? "";
}