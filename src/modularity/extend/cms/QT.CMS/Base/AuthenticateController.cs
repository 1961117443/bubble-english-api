//using QTMall.Core.IServices;
//using QTMall.Core.Model.Models;
//using QTMall.Core.Model.ViewModels;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;

//namespace QTMall.Core.API.Controllers
//{
//    /// <summary>
//    /// 用户登录接口
//    /// </summary>
//    [Route("auth")]
//    [ApiController]
//    public class AuthenticateController : ControllerBase
//    {
//        private readonly ITokenService _tokenService;
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly RoleManager<ApplicationRole> _roleManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        /// <summary>
//        /// 构造函数依赖注入
//        /// </summary>
//        public AuthenticateController(
//            ITokenService tokenService,
//            UserManager<ApplicationUser> userManager,
//            RoleManager<ApplicationRole> roleManager,
//            SignInManager<ApplicationUser> signInManager)
//        {
//            _tokenService = tokenService;
//            _userManager = userManager;
//            _roleManager = roleManager;
//            _signInManager = signInManager;
//        }

//        /// <summary>
//        /// 刷新Token
//        /// </summary>
//        [AllowAnonymous]
//        [HttpPost("retoken")]
//        public async Task<IActionResult> RefreshToken([FromBody] Tokens token)
//        {
//            var resultDto = await _tokenService.GetRefreshTokenAsync(token.RefreshToken);
//            return Ok(resultDto);
//        }

//        /// <summary>
//        /// 用户名密码登录
//        /// </summary>
//        [AllowAnonymous]
//        [HttpPost("login")]
//        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
//        {
//            var resultDto = await _tokenService.LoginAsync(loginDto);
//            return Ok(resultDto);
//        }

//        /// <summary>
//        /// 手机验证码登录
//        /// </summary>
//        [AllowAnonymous]
//        [HttpPost("login/phone")]
//        public async Task<IActionResult> LoginPhone([FromBody] LoginPhoneDto loginDto)
//        {
//            var resultDto = await _tokenService.PhoneAsync(loginDto);
//            return Ok(resultDto);
//        }

//        /// <summary>
//        /// 重设登录密码
//        /// </summary>
//        [AllowAnonymous]
//        [HttpPost("reset")]
//        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDto modelDto)
//        {
//            await _tokenService.ResetAsync(modelDto);
//            return NoContent();
//        }

//        /// <summary>
//        /// 用户注册
//        /// </summary>
//        /*[AllowAnonymous]
//        [HttpPost("register")]
//        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
//        {
//            //检查角色是否存在
//            var role = await _roleManager.FindByIdAsync(registerDto.RoleId.ToString());
//            if (role == null)
//            {
//                return BadRequest("指定的角色不存在");
//            }
//            //创建用户对象
//            var user = new ApplicationUser()
//            {
//                UserName = registerDto.Email,
//                Email = registerDto.Email,
//                Status = 0
//            };
//            //将用户与角色关联
//            user.UserRoles = new List<ApplicationUserRole>()
//            {
//                new ApplicationUserRole()
//                {
//                    RoleId=role.Id,
//                    UserId=user.Id
//                }
//            };
//            //将用户与会员或管理员信息关联
//            if (role.RoleType == 0)
//            {
//                //如果是普通会角色则新增会员信息表
//                user.Member = new Members()
//                {
//                    UserId = user.Id
//                };
//            }
//            else
//            {
//                //否则新增管理员信息表
//                user.Manager = new Manager()
//                {
//                    UserId = user.Id
//                };
//            }

//            //HASH密码，保存用户
//            var result = await _userManager.CreateAsync(user, registerDto.Password);
//            if (!result.Succeeded)
//            {
//                return BadRequest(result.Errors);
//            }

//            return Ok();
//        }*/

//        /// <summary>
//        /// 注销登录
//        /// </summary>
//        [HttpGet("logout")]
//        public async Task<IActionResult> Logout()
//        {
//            await _signInManager.SignOutAsync();
//            return NoContent();
//        }
//    }
//}

using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using QT.CMS.Emum;
using QT.CMS.Entitys.Dto.Login;
using QT.Common.Const;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Core.Security;
using QT.Common.Net;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DynamicApiController;
using QT.EventBus;
using QT.EventHandler;
using QT.Logging.Attributes;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ErrorCode = QT.Common.Enum.ErrorCode;

namespace QT.CMS;

/// <summary>
/// 业务实现：登录服务.
/// </summary>
[ApiDescriptionSettings(ModuleConst.CMS, Tag = "CMS", Name = "Auth", Order = 200)]
[Route("api/cms/[controller]")]
public class AuthenticateController : IDynamicApiController
{
    private readonly ISqlSugarRepository<UserEntity> _repository;
    private readonly ICacheManager _cacheManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEventPublisher _eventPublisher;
    private readonly ITenantManager _tenantManager;
    //private readonly Func<string, ITransient, object> _resolveNamed;

    public AuthenticateController(ISqlSugarRepository<UserEntity> repository, ICacheManager cacheManager, IHttpContextAccessor httpContextAccessor, IEventPublisher eventPublisher,ITenantManager tenantManager)
    {
        _repository = repository;
        _cacheManager = cacheManager;
        _httpContextAccessor = httpContextAccessor;
        _eventPublisher = eventPublisher;
        _tenantManager = tenantManager;
    }

    /// <summary>
    /// 用户名密码登录
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [IgnoreLog]
    [NonUnify]
    public async Task<Tokens> Login([FromBody] LoginDto loginDto)
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

        //var role = loginDto.LoginIdentity.Adapt<AppLoginUserRole>();
        var password = MD5Encryption.Encrypt(loginDto.Password);

        // 根据用户账号获取用户秘钥
        var user = await _repository.SingleAsync(it => it.Account.Equals(loginDto.UserName) && it.DeleteMark == null);
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
                    //{ nameof(AppLoginUserRole), user.Role },
                    { ClaimConst.TENANTID, _tenantManager.TenantId },
                    //{ nameof(JzrcMemberEntity.RelationId), user.RelationId },
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
        //user.FirstLogIp ??= ip;
        user.FirstLogTime ??= DateTime.Now;
        //user.LastLogIp = ip;
        user.LastLogTime = DateTime.Now;

        await _repository.Context.Updateable<UserEntity>(user).ExecuteCommandAsync();

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

        return new Tokens()
        {
            accessToken = accessToken // string.Format("Bearer {0}", accessToken)
            ,
            refreshToken = refreshToken
        };
    }

    ///// <summary>
    ///// 获取当前登录用户信息.
    ///// </summary>
    ///// <param name="type">Web和App</param>
    ///// <returns></returns>
    //[HttpGet("CurrentUser")]
    //public async Task<CurrentUserLoginInfo> GetCurrentUser()
    //{
    //    CurrentUserLoginInfo currentUserLoginInfo = new CurrentUserLoginInfo();

    //    currentUserLoginInfo.userInfo = await _appUserManager.GetUserInfo();

    //    #region 账号权限

    //    currentUserLoginInfo.permission = LoginUserAuth.Common;
    //    currentUserLoginInfo.permission |= LoginUserAuth.Post;
    //    #endregion

    //    #region 账号菜单
    //    var jsonStr = await _repository.Context.Queryable<SysConfigEntity>().Where(x => x.Key == "jzrc_menus").Select(x => x.Value).FirstAsync();

    //    if (jsonStr.IsNotEmptyOrNull())
    //    {
    //        var menuList = jsonStr.ToObject<List<LoginMenuOutput>>() ?? new List<LoginMenuOutput>();

    //        currentUserLoginInfo.menuList = menuList.Where(x => !x.role.HasValue || x.role == currentUserLoginInfo.userInfo.role).ToList();
    //    }

    //    currentUserLoginInfo.menuList.ForEach(x =>
    //    {
    //        if (x.path.IsNullOrEmpty())
    //        {
    //            x.path = ShortIDGen.NextID();
    //        }
    //    });
    //    #endregion

    //    return currentUserLoginInfo;

    //}

    /// <summary>
    /// 手机验证码登录
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login/phone")]
    [NonUnify]
    public async Task<Tokens> LoginPhone([FromBody] LoginPhoneDto loginDto)
    {
        var resultDto = await this.PhoneAsync(loginDto);
        return (resultDto);
    }

    /// <summary>
    /// 手机验证码登录
    /// </summary>
    private async Task<Tokens> PhoneAsync(LoginPhoneDto modelDto)
    {
        //检查验证码
        var codeKey = string.Format("{0}{1}", CommonConst.CACHEKEYCODE, modelDto.CodeKey);
        var cacheObj = await _cacheManager.GetAsync<string>(codeKey);
        if (cacheObj == null)
        {
            throw Oops.Oh("验证码已过期，请重试");
        }
        var cacheValue = cacheObj.ToString();
        var codeSecret = modelDto.CodeValue; // MD5Encryption.Encrypt(modelDto.Phone + modelDto.CodeValue);
        if (codeSecret != cacheValue)
        {
            throw Oops.Oh("验证码有误，请重新获取");
        }
        //验证完毕，删除验证码
        _cacheManager.Del(codeKey);
        //查找用户
        var user = await _repository.AsQueryable()
            //.Includes(x => x.Member)
            //.Include(x => x.Manager)
            .FirstAsync(x => x.MobilePhone == modelDto.Phone);
        if (user == null)
        {
            throw Oops.Oh("用户不存在或已删除");
        }
        //if (user.Status == 1)
        //{
        //    throw Oops.Oh("账户未验证，请验证通过后登录");
        //}
        //if (user.Status == 2)
        //{
        //    throw Oops.Oh("账户待审核，请等待审核后登录");
        //}
        //if (user.Status == 3)
        //{
        //    throw Oops.Oh("账户异常，请联系管理员");
        //}
        //生成Token
        return await CreateTokenAsync(user);
    }

    /// <summary>
    /// 生成Token以及更新用户信息
    /// </summary>
    private async Task<Tokens> CreateTokenAsync(UserEntity user)
    {
        long tokenTimeout = 120;// sysConfig.tokenTimeout;
        // 生成Token令牌
        string accessToken = JWTEncryption.Encrypt(
                new Dictionary<string, object>
                {
                    { ClaimConst.CLAINMUSERID, user.Id },
                    { ClaimConst.CLAINMACCOUNT, user.Account },
                    { ClaimConst.CLAINMREALNAME, user.RealName },
                    //{ ClaimConst.CLAINMADMINISTRATOR, userAnyPwd.IsAdministrator },
                    //{ nameof(AppLoginUserRole), user.Role },
                    { ClaimConst.TENANTID, _tenantManager.TenantId },
                    //{ nameof(JzrcMemberEntity.RelationId), user.RelationId },
                    //{ ClaimConst.TENANTDBNAME, tenantDbName },
                    //{ ClaimConst.SINGLELOGIN, (int)sysConfig.singleLogin },
                }, tokenTimeout);

        string refreshToken = JWTEncryption.GenerateRefreshToken(accessToken, 30);
        // 设置Swagger自动登录
        _httpContextAccessor.HttpContext.SigninToSwagger(accessToken);

        // 设置刷新Token令牌
        _httpContextAccessor.HttpContext.Response.Headers["x-access-token"] = refreshToken; // 生成刷新Token令牌 
        //返回Token
        return new Tokens(accessToken, refreshToken);
    }
}
