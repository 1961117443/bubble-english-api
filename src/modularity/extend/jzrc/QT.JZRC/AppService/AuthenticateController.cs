using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Net;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.EventBus;
using QT.EventHandler;
using QT.FriendlyException;
using QT.JZRC.Entitys;
using QT.JZRC.Entitys.Dto.AppService;
using QT.JZRC.Entitys.Dto.AppService.Login;
using QT.JZRC.Interfaces;
using QT.Logging.Attributes;
using QT.Systems.Entitys.System;
using SqlSugar;
using Mapster;
using QT.DistributedIDGenerator;
using QT.JZRC.Manager;

namespace QT.JZRC;

/// <summary>
/// 业务实现：登录服务.
/// </summary>
[ApiDescriptionSettings(ModuleConst.JZRC, Tag = "JZRC", Name = "Auth", Order = 200)]
[Route("api/JZRC/[controller]")]
public class AuthenticateController :IDynamicApiController
{
    private readonly ISqlSugarRepository<JzrcTalentEntity> _repository;
    private readonly ICacheManager _cacheManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEventPublisher _eventPublisher;
    private readonly ITenantManager _tenantManager;
    //private readonly Func<string, ITransient, object> _resolveNamed;
    private readonly IJzrcAppUserManager _appUserManager;

    public AuthenticateController(ISqlSugarRepository<JzrcTalentEntity> repository,ICacheManager cacheManager,IHttpContextAccessor httpContextAccessor, IEventPublisher eventPublisher,
        ITenantManager tenantManager, IJzrcAppUserManager appUserManager)
    {
        _repository = repository;
        _cacheManager = cacheManager;
        _httpContextAccessor = httpContextAccessor;
        _eventPublisher = eventPublisher;
        _tenantManager = tenantManager;
        //_resolveNamed = resolveNamed;
        _appUserManager = appUserManager;
    }

    /// <summary>
    /// 用户名密码登录
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [IgnoreLog]
    public async Task<dynamic> Login([FromBody] LoginDto loginDto)
    {
        string imageCode = await _cacheManager.GetAsync<string>(string.Format("{0}{1}", CommonConst.CACHEKEYCODE, loginDto.CodeKey));

        if (imageCode.IsNullOrEmpty())
            throw Oops.Oh(ErrorCode.D1030);
        if (!loginDto.CodeValue.ToLower().Equals(imageCode.ToLower()))
            throw Oops.Oh(ErrorCode.D1029);


        //if (!Enum.TryParse<AppLoginUserRole>(loginDto.LoginIdentity, out var role))
        //{
        //    throw Oops.Oh($"未知的身份[{loginDto.LoginIdentity}]");
        //}

        var role = loginDto.LoginIdentity.Adapt<AppLoginUserRole>();
        var password = MD5Encryption.Encrypt(loginDto.Password);

        // 根据用户账号获取用户秘钥
        var user = await _repository.Context.Queryable<JzrcMemberEntity>().SingleAsync(it => it.Account.Equals(loginDto.UserName) && it.Role.Equals((int)role) && it.DeleteMark == null);
        _ = user ?? throw Oops.Oh(ErrorCode.D5002);

        // 获取加密后的密码
        var encryptPasswod = MD5Encryption.Encrypt(password + user.Secretkey);


        // 账户密码是否匹配
        //var userAnyPwd = await _sqlSugarClient.Queryable<UserEntity>().SingleAsync(u => u.Account == input.account && u.Password == encryptPasswod && u.DeleteMark == null);
        if (encryptPasswod != user.Password)
        {
            // 如果是密码错误 记录账号的密码错误次数
            //await UpdateErrorLog(user, sysConfig);
            user.LogErrorCount++;
            await _repository.Context.Updateable(user).UpdateColumns(it => new { it.LogErrorCount }).ExecuteCommandAsync();
            throw Oops.Oh(ErrorCode.D1000);
        }
        else
        {
            _repository.Context.Tracking(user);
            // 清空记录错误次数
            user.LogErrorCount = 0;


            //// 解除锁定
            ////member.EnabledMark = 1;
            //await _repository.Context.Updateable(user).UpdateColumns(it => new { it.LogErrorCount }).ExecuteCommandAsync();
        }

        //_ = userAnyPwd ?? throw Oops.Oh(ErrorCode.D1000);


        //// 获取用户信息才需要
        //var loginService = _resolveNamed(role.ToString(), default) as IJzrcAppLogin ?? throw Oops.Oh($"未知的身份[{loginDto.LoginIdentity}]");
        //var user = await loginService.Login(loginDto) ?? throw Oops.Oh(ErrorCode.D5002);


        #region 生成token

        //// 读取配置文件
        //var array = new Dictionary<string, string>();
        //var sysConfigData = await _repository.Context.Queryable<SysConfigEntity>().Where(x => x.Category.Equals("SysConfig")).ToListAsync();
        //foreach (var item in sysConfigData)
        //{
        //    if (!array.ContainsKey(item.Key)) array.Add(item.Key, item.Value);
        //}

        //var sysConfig = array.ToObject<SysConfigByOAuthModel>();

        // token过期时间
        long tokenTimeout = 120;// sysConfig.tokenTimeout;
        // 生成Token令牌
        string accessToken = JWTEncryption.Encrypt(
                new Dictionary<string, object>
                {
                    { ClaimConst.CLAINMUSERID, user.Id },
                    { ClaimConst.CLAINMACCOUNT, user.Account },
                    { ClaimConst.CLAINMREALNAME, user.NickName },
                    //{ ClaimConst.CLAINMADMINISTRATOR, userAnyPwd.IsAdministrator },
                    { nameof(AppLoginUserRole), user.Role },
                    { ClaimConst.TENANTID, _tenantManager.TenantId },
                    { nameof(JzrcMemberEntity.RelationId), user.RelationId },
                    //{ ClaimConst.TENANTDBNAME, tenantDbName },
                    //{ ClaimConst.SINGLELOGIN, (int)sysConfig.singleLogin },
                }, tokenTimeout);

        string refreshToken = JWTEncryption.GenerateRefreshToken(accessToken, 30);
        // 设置Swagger自动登录
        _httpContextAccessor.HttpContext.SigninToSwagger(accessToken);

        // 设置刷新Token令牌
        _httpContextAccessor.HttpContext.Response.Headers["x-access-token"] = refreshToken; // 生成刷新Token令牌 
        #endregion

        var ip = NetHelper.Ip;

        // 成功次数
        user.LogSuccessCount++;
        user.FirstLogIp ??= ip;
        user.FirstLogTime ??= DateTime.Now;
        user.LastLogIp = ip;
        user.LastLogTime = DateTime.Now;
        
        await _repository.Context.Updateable<JzrcMemberEntity>(user).ExecuteCommandAsync();

        //// 修改用户登录信息
        //await _eventPublisher.PublishAsync(new UserEventSource("User:UpdateUserLogin", _tenantManager.TenantId, _tenantManager.TenantId, new UserEntity
        //{
        //    Id = user.Id,
        //    FirstLogIP = ip,
        //    FirstLogTime = DateTime.Now,
        //    LastLogTime = DateTime.Now,
        //    LastLogIP = ip,
        //    //LogSuccessCount = user.LogSuccessCount + 1
        //}));

        // 增加登录日志
        await _eventPublisher.PublishAsync(new LogEventSource("Log:CreateVisLog", _tenantManager.TenantId, _tenantManager.TenantId, new SysLogEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            UserId = user.Id,
            UserName = user.NickName,
            Category = 1,
            IPAddress = ip,
            Abstracts = "登录成功",
            PlatForm = string.Format("{0}-{1}", UserAgent.GetSystem(), UserAgent.GetBrowser()),
            CreatorTime = DateTime.Now
        }));

        return new
        {
            accessToken // string.Format("Bearer {0}", accessToken)
            , refreshToken
        };
    }

    /// <summary>
    /// 获取当前登录用户信息.
    /// </summary>
    /// <param name="type">Web和App</param>
    /// <returns></returns>
    [HttpGet("CurrentUser")]
    public async Task<CurrentUserLoginInfo> GetCurrentUser()
    {
        CurrentUserLoginInfo currentUserLoginInfo = new CurrentUserLoginInfo();
        
        currentUserLoginInfo.userInfo = await _appUserManager.GetUserInfo();

        #region 账号权限

        currentUserLoginInfo.permission = LoginUserAuth.Common;
        currentUserLoginInfo.permission |= LoginUserAuth.Post;
        #endregion

        #region 账号菜单
        var jsonStr = await _repository.Context.Queryable<SysConfigEntity>().Where(x => x.Key == "jzrc_menus").Select(x => x.Value).FirstAsync();

        if (jsonStr.IsNotEmptyOrNull())
        {
            var menuList = jsonStr.ToObject<List<LoginMenuOutput>>() ?? new List<LoginMenuOutput>();

            currentUserLoginInfo.menuList = menuList.Where(x => !x.role.HasValue || x.role == currentUserLoginInfo.userInfo.role).ToList();
        }

        currentUserLoginInfo.menuList.ForEach(x =>
        {
            if (x.path.IsNullOrEmpty())
            {
                x.path = ShortIDGen.NextID();
            }
        }); 
        #endregion

        return currentUserLoginInfo;

    }
}
