using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QT.Common.Cache;
using QT.Common.Const;
using QT.Common.Core;
using QT.Common.Core.Captcha.General;
using QT.Common.Core.Configs;
using QT.Common.Core.Dto.IWechatAppProxy;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Core.Security;
using QT.Common.Core.Service;
using QT.Common.Dtos.OAuth;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Models.User;
using QT.Common.Net;
using QT.Common.Options;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DataValidation;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.EventBus;
using QT.EventHandler;
using QT.Extras.Thirdparty.Sms;
using QT.FriendlyException;
using QT.JsonSerialization;
using QT.Logging.Attributes;
using QT.OAuth.Dto;
using QT.OAuth.Model;
using QT.RemoteRequest.Extensions;
using QT.Systems.Entitys;
using QT.Systems.Entitys.Dto.SysConfig;
using QT.Systems.Entitys.Dto.User;
using QT.Systems.Entitys.Enum;
using QT.Systems.Entitys.Model.SysConfig;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.Permission;
using QT.Systems.Interfaces.System;
using QT.UnifyResult;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace QT.OAuth;

/// <summary>
/// 业务实现：身份认证模块.
/// </summary>
[ApiDescriptionSettings(Tag = "OAuth", Name = "OAuth", Order = 160)]
[Route("api/[controller]")]
public class OAuthService : IDynamicApiController, IScoped
{
    /// <summary>
    /// 用户仓储.
    /// </summary>
    private readonly ISqlSugarRepository<UserEntity> _userRepository;

    /// <summary>
    /// 功能模块.
    /// </summary>
    private readonly IModuleService _moduleService;

    /// <summary>
    /// 功能按钮.
    /// </summary>
    private readonly IModuleButtonService _moduleButtonService;

    /// <summary>
    /// 功能列.
    /// </summary>
    private readonly IModuleColumnService _columnService;

    /// <summary>
    /// 功能数据权限计划.
    /// </summary>
    private readonly IModuleDataAuthorizeSchemeService _moduleDataAuthorizeSchemeService;

    /// <summary>
    /// 功能表单.
    /// </summary>
    private readonly IModuleFormService _formService;

    /// <summary>
    /// 系统配置.
    /// </summary>
    private readonly ISysConfigService _sysConfigService;

    /// <summary>
    /// 验证码处理程序.
    /// </summary>
    private readonly IGeneralCaptcha _captchaHandler;

    /// <summary>
    /// 数据库配置选项.
    /// </summary>
    private readonly ConnectionStringsOptions _connectionStrings;

    /// <summary>
    /// 多租户配置选项.
    /// </summary>
    private readonly TenantOptions _tenant;

    /// <summary>
    /// Http上下文.
    /// </summary>
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 缓存管理.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 事件总线.
    /// </summary>
    private readonly IEventPublisher _eventPublisher;
    private readonly IUsersService _usersService;
    private readonly ITenantManager _tenantManager;
    private readonly ICoreSysConfigService _coreSysConfigService;

    /// <summary>
    /// SqlSugarClient客户端.
    /// </summary>
    private SqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 初始化一个<see cref="OAuthService"/>类型的新实例.
    /// </summary>
    public OAuthService(
        IGeneralCaptcha captchaHandler,
        ISqlSugarRepository<UserEntity> userRepository,
        IModuleService moduleService,
        IModuleButtonService moduleButtonService,
        IModuleColumnService columnService,
        IModuleDataAuthorizeSchemeService moduleDataAuthorizeSchemeService,
        IModuleFormService formService,
        ISysConfigService sysConfigService,
        IOptions<ConnectionStringsOptions> connectionOptions,
        IOptions<TenantOptions> tenantOptions,
        ISqlSugarClient sqlSugarClient,
        IHttpContextAccessor httpContextAccessor,
        ICacheManager cacheManager,
        IUserManager userManager,
        IEventPublisher eventPublisher,
        IUsersService usersService,
        ITenantManager tenantManager,
        ICoreSysConfigService coreSysConfigService)
    {
        _captchaHandler = captchaHandler;
        _userRepository = userRepository;
        _moduleService = moduleService;
        _moduleButtonService = moduleButtonService;
        _columnService = columnService;
        _moduleDataAuthorizeSchemeService = moduleDataAuthorizeSchemeService;
        _formService = formService;
        _sysConfigService = sysConfigService;
        _connectionStrings = connectionOptions.Value;
        _tenant = tenantOptions.Value;
        _sqlSugarClient = (SqlSugarClient)sqlSugarClient;
        _httpContextAccessor = httpContextAccessor;
        _cacheManager = cacheManager;
        _userManager = userManager;
        _eventPublisher = eventPublisher;
        _usersService = usersService;
        _tenantManager = tenantManager;
        _coreSysConfigService = coreSysConfigService;
    }

    /// <summary>
    /// 获取图形验证码.
    /// </summary>
    /// <param name="codeLength">验证码长度.</param>
    /// <param name="timestamp">时间戳.</param>
    /// <returns></returns>
    [HttpGet("ImageCode/{codeLength}/{timestamp}")]
    [AllowAnonymous]
    [IgnoreLog]
    [NonUnify]
    public async Task<IActionResult> GetCode(int codeLength, string timestamp)
    {
        return new FileContentResult(await _captchaHandler.CreateCaptchaImage(timestamp, 120, 40, codeLength > 0 ? codeLength : 4), "image/jpeg");
    }

    /// <summary>
    /// 获取短信验证码.
    /// </summary>
    /// <param name="codeLength">验证码长度.</param>
    /// <param name="phone">手机号.</param>
    /// <returns></returns>
    [HttpGet("SmsCode/{codeLength}/{phone}")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task GetSmsCode(int codeLength, [Required] string phone)
    {
        if (!phone.TryValidate(ValidationTypes.PhoneNumber).IsValid)
        {
            throw Oops.Oh("请输入正确的手机号码！");
        }
        string cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYCODE, phone);
        if (_cacheManager.Exists($"{cacheKey}_timestamp"))
        {
            throw Oops.Oh("请稍后再操作！");
        }

        var entity = await _userRepository.Context.Queryable<SmsTemplateEntity>().SingleAsync(x => x.EnCode == "login.verificationcode") ?? throw Oops.Oh("缺少登录短信模板[login.verificationcode]");

        var code = new Random().NextNumberString(codeLength);
        // 发送短信
        var sysconfig = await _sysConfigService.GetInfo();
        var smsModel = new SmsParameterInfo()
        {
            keyId = entity.Company == 1 ? sysconfig.aliAccessKey : sysconfig.tencentSecretId,
            keySecret = entity.Company == 1 ? sysconfig.aliSecret : sysconfig.tencentSecretKey,
            region = entity.Region,
            domain = entity.Endpoint,
            templateId = entity.TemplateId,
            signName = entity.SignContent
        };

        var msg = string.Empty;
        if (entity.Company == 1)
        {
            smsModel.mobileAli = phone;
            smsModel.templateParamAli = (new { code }).ToJsonString();
            msg = SmsUtil.SendSmsByAli(smsModel);
        }
        else
        {
            smsModel.mobileTx = new string[] { phone };
            List<string> mList = new List<string>();
            //foreach (string data in input.parameters.Values)
            //{
            //    mList.Add(data);
            //}
            smsModel.appId = sysconfig.tencentAppId;
            smsModel.templateParamTx = mList.ToArray();
            msg = SmsUtil.SendSmsByTencent(smsModel);
        }

        if (msg.Equals("短信发送失败"))
            throw Oops.Oh(msg);

        // 写入缓存
        // 一分钟之内只能发送一次
        await _cacheManager.SetAsync($"{cacheKey}_timestamp", DateTime.Now.ParseToUnixTime(), TimeSpan.FromMinutes(1));

        // 验证码5分钟内有效
        await _cacheManager.SetAsync(cacheKey, code, TimeSpan.FromMinutes(5));
    }

    #region Get

    /// <summary>
    /// 首次登录 根据账号获取数据库配置.
    /// </summary>
    /// <param name="account">账号.</param>
    [HttpGet("getConfig/{account}")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task<dynamic> GetConfigCode(string account)
    {
        if (_tenant.MultiTenancy)
        {
            string tenantDbName = _connectionStrings.DBName;
            string tenantId = _connectionStrings.ConfigId;
            string tenantAccout = string.Empty;

            if (!string.IsNullOrEmpty(_tenantManager.TenantId))
            {
                tenantId = _tenantManager.TenantId;
                tenantAccout = account;
            }
            else
            {
                //分割账号
                var tenantAccount = account.Split('@');
                tenantId = tenantAccount.FirstOrDefault();

                if (tenantAccount.Length == 1) account = CommonConst.SUPPER_ADMIN_ACCOUNT;
                else account = tenantAccount[1];

                tenantAccout = account;
            }

            if (!_tenantManager.IsLoggedIn)
            {
                var interFace = string.Format("{0}{1}", _tenant.MultiTenancyDBInterFace, tenantId);

                //interFace.set
                var response = await interFace.GetAsStringAsync();
                var result = response.ToObject<RESTfulResult<TenantInterFaceOutput>>();
                if (result.code != 200)
                    throw Oops.Oh(result.msg);
                else if (result.data.dotnet == null)
                    throw Oops.Oh(ErrorCode.D1025);
                else
                    tenantDbName = result.data.dotnet;

                var connectionConfig = SqlSugarHelper.CreateConnectionConfig(tenantId, result.data);
                _sqlSugarClient.AddConnection(connectionConfig);
            }


            //_sqlSugarClient.AddConnection(new ConnectionConfig()
            //{
            //    DbType = (DbType)Enum.Parse(typeof(DbType), _connectionStrings.DBType),
            //    ConfigId = tenantId, // 设置库的唯一标识
            //    IsAutoCloseConnection = true,
            //    ConnectionString = string.Format(_connectionStrings.DefaultConnection, tenantDbName)
            //});
            _sqlSugarClient.ChangeDatabase(tenantId);
        }

        // 验证连接是否成功
        if (!_sqlSugarClient.Ado.IsValidConnection())
        {
            throw Oops.Oh(ErrorCode.D1032);
        }

        // 读取配置文件
        var array = new Dictionary<string, object>();
        var sysConfigData = await _sqlSugarClient.Queryable<SysConfigEntity>().Where(x => x.Category.Equals("SysConfig") && (x.Key.Equals("enableverificationcode") || x.Key.Equals("verificationcodenumber"))).ToListAsync();
        foreach (var item in sysConfigData)
        {
            if (!array.ContainsKey(item.Key)) array.Add(item.Key, item.Value);
        }

        var sysConfig = array.ToObject<SysConfigModel>();

        // 返回给前端 是否开启验证码 和 验证码长度
        return new { enableVerificationCode = sysConfig.enableVerificationCode, verificationCodeNumber = sysConfig.verificationCodeNumber > 0 ? sysConfig.verificationCodeNumber : 4 };
    }

    /// <summary>
    /// 获取当前登录用户信息.
    /// </summary>
    /// <param name="type">Web和App</param>
    /// <returns></returns>
    [HttpGet("CurrentUser")]
    public async Task<dynamic> GetCurrentUser(string type)
    {
        if (type.IsNullOrEmpty()) type = "Web"; // 默认为Web端菜单目录
        await _cacheManager.DelAsync($"u:{_userManager.UserId}:GetUserModueList"); // 删除缓存
        var userId = _userManager.UserId;
        var tenantId = _userManager.TenantId;
        var tenantDbName = _userManager.TenantDbName;
        var sp1 = Stopwatch.StartNew();
        var loginOutput = new CurrentUserOutput();

        // 系统配置信息，前置，减少查询
        //var sysInfo = await _sysConfigService.GetInfo();
        var sysInfo = await _coreSysConfigService.GetSysConfig();
        loginOutput.sysConfigInfo = sysInfo.Adapt<SysConfigInfo>();


        loginOutput.userInfo = await _userManager.GetUserInfo();
        sp1.Stop();

        Console.WriteLine("获取用户信息耗时：{0}", sp1.ElapsedMilliseconds);
        var sp2 = Stopwatch.StartNew();
        // 菜单
        loginOutput.menuList = await _moduleService.GetUserTreeModuleList(type);

        var currentUserModel = new CurrentUserModelOutput();
        currentUserModel.moduleList = await _moduleService.GetUserModueList(type);
        currentUserModel.buttonList = await _moduleButtonService.GetUserModuleButtonList();
        currentUserModel.columnList = await _columnService.GetUserModuleColumnList();
        currentUserModel.resourceList = await _moduleDataAuthorizeSchemeService.GetResourceList();
        currentUserModel.formList = await _formService.GetUserModuleFormList();

        // 权限信息
        var permissionList = new List<PermissionModel>();
        currentUserModel.moduleList.ForEach(menu =>
        {
            var permissionModel = new PermissionModel();
            permissionModel.modelId = menu.id;
            permissionModel.moduleName = menu.fullName;
            permissionModel.moduleCode = menu.enCode;
            permissionModel.button = currentUserModel.buttonList.FindAll(t => t.moduleId.Equals(menu.id)).Adapt<List<FunctionalButtonAuthorizeModel>>();
            permissionModel.column = currentUserModel.columnList.FindAll(t => t.moduleId.Equals(menu.id)).Adapt<List<FunctionalColumnAuthorizeModel>>();
            permissionModel.form = currentUserModel.formList.FindAll(t => t.moduleId.Equals(menu.id)).Adapt<List<FunctionalFormAuthorizeModel>>();
            permissionModel.resource = currentUserModel.resourceList.FindAll(t => t.moduleId.Equals(menu.id)).Adapt<List<FunctionalResourceAuthorizeModel>>();
            permissionList.Add(permissionModel);
        });

        loginOutput.permissionList = permissionList;
        sp2.Stop();
        Console.WriteLine("获取用户权限耗时：{0}", sp2.ElapsedMilliseconds);

        //获取用户绑定的公司
        var olist = await _usersService.GetRelationOrganizeList(loginOutput.userInfo.userId);
        if (olist != null)
        {
            loginOutput.userInfo.companys = olist.Select(x => new CompanyInfoModel { id = x.Id, name = x.FullName, isJt = x.EnCode.ToUpper() == "JT" }).ToList();
        }
        else
        {
            loginOutput.userInfo.companys = new List<CompanyInfoModel>();
        }
        ////
        //string cacheKey = string.Format("{0}{1}_{2}", CommonConst.CACHEKEYONLINEUSER, loginOutput.userInfo.userId, "company");
        ////Debug.WriteLine("{0}", _cacheManager.Get("abcd"));
        //var org = await _cacheManager.GetAsync<OrganizeEntity>(cacheKey);
        //loginOutput.userInfo.companyId = org?.Id; // loginOutput.userInfo.companys.FirstOrDefault(x => x.id == companyId) ?? new CompanyInfoModel();

        loginOutput.userInfo.companyId = loginOutput.userInfo?.organizeId ?? _userManager.CompanyId;

        // 缓存当前权限
        await _cacheManager.SetAsync(string.Format("{0}{1}_{2}", CommonConst.CACHEKEYONLINEUSER, loginOutput.userInfo.userId, $"auth_{type.ToLower()}"), loginOutput);
        return loginOutput;
    }

    /// <summary>
    /// 退出.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Logout")]
    public async Task Logout()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        httpContext.SignoutToSwagger();

        // 清除IM中的webSocket
        var list = await GetOnlineUserList();
        if (list != null)
        {
            var onlineUser = list.Find(it => it.tenantId == _userManager.TenantId && it.userId == _userManager.UserId);
            if (onlineUser != null)
            {
                list.RemoveAll((x) => x.connectionId == onlineUser.connectionId);
                await SetOnlineUserList(list);
            }
        }

        await DelUserInfo();
    }

    /// <summary>
    /// 检验租户账号是否有效
    /// </summary>
    /// <param name="account"></param>
    /// <returns></returns>
    [HttpGet("saas/{account}")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task<string> SaasCheck(string account)
    {
        var interFace = string.Format("{0}{1}", _tenant.MultiTenancyDBInterFace, account);

        //interFace.set
        var response = await interFace.GetAsStringAsync();
        var result = response.ToObject<RESTfulResult<TenantInterFaceOutput>>();
        if (result.code != 200)
            throw Oops.Oh("单位不存在");
        //throw Oops.Oh(result.msg);
        else if (result.data.dotnet == null)
            throw Oops.Oh(ErrorCode.D1025);
        //else
        //    tenantDbName = result.data.dotnet;

        var connectionConfig = SqlSugarHelper.CreateConnectionConfig(account, result.data);

        TenantScopeModel tenantScopeModel = result.data.Adapt<TenantScopeModel>();
        tenantScopeModel.Code = account;
        _sqlSugarClient.AddConnection(connectionConfig);
        //_sqlSugarClient.ChangeDatabase(account);

        // 加密对象
        var encrypt = DESCEncryption.Encrypt(JSON.Serialize(tenantScopeModel), "QT");
        // 写入响应头
        App.HttpContext?.Response.Headers.Add("x-saas-token", encrypt);

        return encrypt;
    }
    #endregion

    #region POST

    /// <summary>
    /// 用户登录.
    /// </summary>
    /// <param name="input">登录输入参数.</param>
    /// <returns></returns>
    [HttpPost("Login")]
    [Consumes("application/x-www-form-urlencoded")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task<dynamic> Login([FromForm] LoginInput input)
    {
        string tenantDbName = _connectionStrings.DBName;
        string tenantId = _connectionStrings.ConfigId;
        string tenantAccout = string.Empty;

        if (_tenant.MultiTenancy)
        {
            if (_tenantManager.IsLoggedIn)
            {
                tenantId = _tenantManager.TenantId;
                tenantAccout = input.account;
            }
            else
            {
                // 分割账号
                var tenantAccount = input.account.Split('@');
                tenantId = tenantAccount.FirstOrDefault();
                if (tenantAccount.Length == 1)
                    input.account = "admin";
                else
                    input.account = tenantAccount[1];
                tenantAccout = input.account;
            }


            var interFace = string.Format("{0}{1}", _tenant.MultiTenancyDBInterFace, tenantId);
            var response = await interFace.GetAsStringAsync();
            var result = response.ToObject<RESTfulResult<TenantInterFaceOutput>>();
            if (result.code != 200)
                throw Oops.Oh(result.msg);
            else if (result.data.dotnet == null)
                throw Oops.Oh(ErrorCode.D1025);
            else
                tenantDbName = result.data.dotnet;

            if (!_sqlSugarClient.IsAnyConnection(tenantId))
            {
                _sqlSugarClient.AddConnection(SqlSugarHelper.CreateConnectionConfig(tenantId, result.data));
            }

            //_sqlSugarClient.AddConnection(new ConnectionConfig()
            //{
            //    DbType = (DbType)Enum.Parse(typeof(DbType), _connectionStrings.DBType),
            //    ConfigId = tenantId, // 设置库的唯一标识
            //    IsAutoCloseConnection = true,
            //    ConnectionString = string.Format(_connectionStrings.DefaultConnection, tenantDbName)
            //});
            _sqlSugarClient.ChangeDatabase(tenantId);
        }

        // 验证连接是否成功
        if (!_sqlSugarClient.Ado.IsValidConnection())
        {
            throw Oops.Oh(ErrorCode.D1032);
        }

        // 读取配置文件
        var array = new Dictionary<string, string>();
        var sysConfigData = await _sqlSugarClient.Queryable<SysConfigEntity>().Where(x => x.Category.Equals("SysConfig")).ToListAsync();
        foreach (var item in sysConfigData)
        {
            if (!array.ContainsKey(item.Key)) array.Add(item.Key, item.Value);
        }

        var sysConfig = array.ToObject<SysConfigByOAuthModel>();

        if (sysConfig.enableVerificationCode && string.IsNullOrEmpty(input.code))
        {
            throw Oops.Oh("请输入验证码.");
        }
        // 验证码不为空时
        if (!string.IsNullOrEmpty(input.code))
        {
            if (sysConfig.enableVerificationCode)
            {
                if (string.IsNullOrWhiteSpace(input.timestamp) || string.IsNullOrWhiteSpace(input.code))
                    throw Oops.Oh(ErrorCode.D1029);
                string imageCode = await GetCode(input.timestamp);
                if (imageCode.IsNullOrEmpty())
                    throw Oops.Oh(ErrorCode.D1030);
                if (!input.code.ToLower().Equals(imageCode.ToLower()))
                    throw Oops.Oh(ErrorCode.D1029);
            }
        }

        // 根据用户账号获取用户秘钥
        var user = await _sqlSugarClient.Queryable<UserEntity>().SingleAsync(it => it.Account.Equals(input.account) && it.DeleteMark == null);
        _ = user ?? throw Oops.Oh(ErrorCode.D5002);

        // 验证账号是否未被激活
        if (user.EnabledMark == null) throw Oops.Oh(ErrorCode.D1018);

        // 验证账号是否被禁用
        if (user.EnabledMark == 0) throw Oops.Oh(ErrorCode.D1019);

        // 验证账号是否被删除
        if (user.DeleteMark == 1) throw Oops.Oh(ErrorCode.D1017);

        if (user.ExpireTime.HasValue && DateTime.Now > user.ExpireTime)
        {
            UnifyContext.Fill(new { uid = user.Id });
            throw Oops.Oh(ErrorCode.D5024).StatusCode((int)HttpStatusCode.AccountExpiration);
        }

        // 是否延迟登录
        if (sysConfig.lockType.Equals(ErrorStrategy.Delay) && user.UnLockTime.IsNullOrEmpty())
        {
            if (user.UnLockTime > DateTime.Now)
            {
                int unlockTime = ((user.UnLockTime - DateTime.Now)?.TotalMinutes).ParseToInt();
                if (unlockTime < 1) unlockTime = 1;
                throw Oops.Oh(ErrorCode.D1027, unlockTime);
            }
            else if (user.UnLockTime <= DateTime.Now)
            {
                user.EnabledMark = 1;
                user.LogErrorCount = 0;
                await _sqlSugarClient.Updateable(user).UpdateColumns(it => new { it.LogErrorCount, it.EnabledMark }).ExecuteCommandAsync();
            }
        }

        // 是否 延迟登录
        if (sysConfig.lockType.Equals(ErrorStrategy.Delay) && user.UnLockTime.IsNotEmptyOrNull() && user.UnLockTime > DateTime.Now)
        {
            int? t3 = (user.UnLockTime - DateTime.Now)?.TotalMinutes.ParseToInt();
            if (t3 < 1) t3 = 1;
            throw Oops.Oh(ErrorCode.D1027, t3?.ToString());
        }

        if (sysConfig.lockType.Equals(ErrorStrategy.Delay) && user.UnLockTime.IsNotEmptyOrNull() && user.UnLockTime <= DateTime.Now)
        {
            user.EnabledMark = 1;
            user.LogErrorCount = 0;
            await _sqlSugarClient.Updateable(user).UpdateColumns(it => new { it.LogErrorCount, it.EnabledMark }).ExecuteCommandAsync();
        }

        // 是否锁定
        if (user.EnabledMark == 2) throw Oops.Oh(ErrorCode.D1031);

        // 获取加密后的密码
        var encryptPasswod = MD5Encryption.Encrypt(input.password + user.Secretkey);

        // 账户密码是否匹配
        var userAnyPwd = await _sqlSugarClient.Queryable<UserEntity>().SingleAsync(u => u.Account == input.account && u.Password == encryptPasswod && u.DeleteMark == null);
        if (userAnyPwd.IsNullOrEmpty())
        {
            // 如果是密码错误 记录账号的密码错误次数
            await UpdateErrorLog(user, sysConfig);
        }
        else
        {
            // 清空记录错误次数
            userAnyPwd.LogErrorCount = 0;

            // 解除锁定
            userAnyPwd.EnabledMark = 1;
            await _sqlSugarClient.Updateable(userAnyPwd).UpdateColumns(it => new { it.LogErrorCount, it.EnabledMark }).ExecuteCommandAsync();
        }

        _ = userAnyPwd ?? throw Oops.Oh(ErrorCode.D1000);

        // app权限验证
        if (UserAgent.isMobileBrowser && user.IsAdministrator == 0 && !ExistRoleByApp(user.RoleId))
            throw Oops.Oh(ErrorCode.D1022);

        // 登录成功时 判断单点登录信息
        int whitelistSwitch = Convert.ToInt32(sysConfig.whitelistSwitch);
        string whiteListIp = sysConfig.whiteListIp;
        if (whitelistSwitch.Equals(1) && user.IsAdministrator.Equals(0))
        {
            if (!whiteListIp.Split(",").Contains(NetHelper.Ip))
                throw Oops.Oh(ErrorCode.D9002);
        }

        // token过期时间
        long tokenTimeout = sysConfig.tokenTimeout;

        // 绑定默认角色，
        // 绑定规则：
        // 0、只有一个角色的情况才默认绑定
        // 1、lastRoleId没有值 或者 lastRoleId 不在 roleid 里面
        bool switchRole = false; // 是否需要选择角色
        var roles = user.RoleId?.Split(",", StringSplitOptions.RemoveEmptyEntries)?.ToList() ?? new List<string>();
        // 判断角色是否有授权
        if (roles.IsAny() && roles.Count > 1)
        {
            var items = await _sqlSugarClient.Context.Queryable<AuthorizeEntity>()
                   .In(a => a.ObjectId, roles)
                   .Where(a => a.ItemType == "module")
                   .Where(a => SqlFunc.Subqueryable<ModuleEntity>().Where(d => d.Id == a.ItemId && d.EnabledMark == 1 && d.DeleteMark == null).Any())
                   .ToListAsync();

            // 获取禁用的权限
            var disableItems = await _sqlSugarClient.Context.Queryable<AuthorizeDisableEntity>()
                .In(a => a.ObjectId, roles)
                .Where(a => a.ItemType == "module")
                .Where(a => SqlFunc.Subqueryable<ModuleEntity>().Where(d => d.Id == a.ItemId && d.EnabledMark == 1 && d.DeleteMark == null).Any())
                .ToListAsync();

            // 排除掉禁用的权限
            if (disableItems.IsAny())
            {
                items = items.Where(it => !disableItems.Any(x => x.ObjectId == it.ObjectId && x.ItemId == it.ItemId)).ToList();
            }

            roles = items.Select(it => it.ObjectId).Distinct().ToList();
        }
        if (!string.IsNullOrEmpty(user.LastRoleId) && (roles.Count == 0 || !roles.Contains(user.LastRoleId)))
        {
            user.LastRoleId = string.Empty;
        }
        if (string.IsNullOrEmpty(user.LastRoleId) && roles.Count > 0)
        {
            // 如果多角色的普通用户没有登录过系统（LastRoleId 为空）,需要选择角色
            switchRole = user.IsAdministrator.Equals(0) && roles.Count > 1;
            user.LastRoleId = roles[0];
        }
        await _sqlSugarClient.Context.Updateable<UserEntity>(user).UpdateColumns(it => it.LastRoleId).ExecuteCommandAsync();


        // 判断是否有绑定公司，默认选择第一个
        //获取用户绑定的公司
        var olist = await _usersService.GetRelationOrganizeList(userAnyPwd.Id);
        if (olist != null && olist.Any())
        {
            string cacheKey = string.Format("{0}{1}_{2}", CommonConst.CACHEKEYONLINEUSER, userAnyPwd.Id, "company");
            var org = await _cacheManager.GetAsync<OrganizeEntity>(cacheKey);
            // 不存在才设置第一个
            if (org == null || !olist.Any(x => x.Id == org.Id))
            {
                await _cacheManager.SetAsync(cacheKey, olist[0]);
            }
        }

        // 生成Token令牌
        string accessToken = JWTEncryption.Encrypt(
                new Dictionary<string, object>
                {
                    { ClaimConst.CLAINMUSERID, userAnyPwd.Id },
                    { ClaimConst.CLAINMACCOUNT, userAnyPwd.Account },
                    { ClaimConst.CLAINMREALNAME, userAnyPwd.RealName },
                    { ClaimConst.CLAINMADMINISTRATOR, userAnyPwd.IsAdministrator },
                    { ClaimConst.TENANTID, tenantId },
                    { ClaimConst.TENANTDBNAME, tenantDbName },
                    { ClaimConst.SINGLELOGIN, (int)sysConfig.singleLogin },
                    //{ ClaimConst.CLAINMCOMPANYID, companyId },
                    //{ ClaimConst.CLAINM_JT_COMPANY_ACCOUNT, isJT?1:0 }
                }, tokenTimeout);

        // 设置Swagger自动登录
        _httpContextAccessor.HttpContext.SigninToSwagger(accessToken);

        // 设置刷新Token令牌
        _httpContextAccessor.HttpContext.Response.Headers["x-access-token"] = JWTEncryption.GenerateRefreshToken(accessToken, 30); // 生成刷新Token令牌

        var ip = NetHelper.Ip;

        // 修改用户登录信息
        await _eventPublisher.PublishAsync(new UserEventSource("User:UpdateUserLogin", tenantId, tenantDbName, new UserEntity
        {
            Id = user.Id,
            FirstLogIP = user.FirstLogIP ?? ip,
            FirstLogTime = user.FirstLogTime ?? DateTime.Now,
            PrevLogTime = user.LastLogTime,
            PrevLogIP = user.LastLogIP,
            LastLogTime = DateTime.Now,
            LastLogIP = ip,
            LogSuccessCount = user.LogSuccessCount + 1
        }));

        // 增加登录日志
        await _eventPublisher.PublishAsync(new LogEventSource("Log:CreateVisLog", tenantId, tenantDbName, new SysLogEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            UserId = user.Id,
            UserName = user.RealName,
            Category = 1,
            IPAddress = ip,
            Abstracts = "登录成功",
            PlatForm = string.Format("{0}-{1}", UserAgent.GetSystem(), UserAgent.GetBrowser()),
            CreatorTime = DateTime.Now
        }));

        return new
        {
            theme = user.Theme == null ? "classic" : user.Theme,
            token = string.Format("Bearer {0}", accessToken),
            switchRole = switchRole
        };
    }

    /// <summary>
    /// 锁屏解锁登录.
    /// </summary>
    /// <param name="input">登录输入参数.</param>
    /// <returns></returns>
    [HttpPost("LockScreen")]
    public async Task LockScreen([FromBody] LockScreenInput input)
    {
        // 根据用户账号获取用户秘钥
        var secretkey = (await _userRepository.FirstOrDefaultAsync(u => u.Account == input.account && u.DeleteMark == null)).Secretkey;

        // 获取加密后的密码
        var encryptPasswod = MD5Encryption.Encrypt(input.password + secretkey);

        var user = await _userRepository.FirstOrDefaultAsync(u => u.Account == input.account && u.Password == encryptPasswod && u.DeleteMark == null);
        _ = user ?? throw Oops.Oh(ErrorCode.D1000);
    }

    #endregion

    /// <summary>
    /// input.account = 手机号码，input.code 为验证码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("LoginByPhoneNumber")]
    [Consumes("application/x-www-form-urlencoded")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task<dynamic> LoginByPhoneNumber([FromForm] LoginInput input)
    {
        string tenantDbName = _connectionStrings.DBName;
        string tenantId = _connectionStrings.ConfigId;

        if (_tenant.MultiTenancy)
        {
            if (_tenantManager.IsLoggedIn)
            {
                tenantId = _tenantManager.TenantId;
            }


            var interFace = string.Format("{0}{1}", _tenant.MultiTenancyDBInterFace, tenantId);
            var response = await interFace.GetAsStringAsync();
            var result = response.ToObject<RESTfulResult<TenantInterFaceOutput>>();
            if (result.code != 200)
                throw Oops.Oh(result.msg);
            else if (result.data.dotnet == null)
                throw Oops.Oh(ErrorCode.D1025);
            else
                tenantDbName = result.data.dotnet;

            if (!_sqlSugarClient.IsAnyConnection(tenantId))
            {
                _sqlSugarClient.AddConnection(SqlSugarHelper.CreateConnectionConfig(tenantId, result.data));
            }
            _sqlSugarClient.ChangeDatabase(tenantId);
        }

        // 手机登录需要验证码
        if (string.IsNullOrEmpty(input.code))
        {
            throw Oops.Oh("请输入验证码.");
        }

        if (!string.IsNullOrEmpty(input.code))
        {
            if (string.IsNullOrWhiteSpace(input.timestamp) || string.IsNullOrWhiteSpace(input.code))
                throw Oops.Oh(ErrorCode.D1029);
            string imageCode = await GetCode(input.timestamp);
            if (imageCode.IsNullOrEmpty())
                throw Oops.Oh(ErrorCode.D1030);
            if (!input.code.ToLower().Equals(imageCode.ToLower()))
                throw Oops.Oh(ErrorCode.D1029);
        }

        #region 自动创建账号
        // 判断账号是否存在
        var phoneNumber = input.account;
        if (!await _userRepository.AnyAsync(x => x.MobilePhone == phoneNumber))
        {
            var config = await _coreSysConfigService.GetConfig<IotConfigs>();
            //var config = await _userRepository.Context.Queryable<SysConfigEntity>().Where(x => x.Key == nameof(SysConfigOutput.defaultRoleId)).FirstAsync();
            if (config != null && config.isUseDemoRole && config.defaultRoleId.IsNotEmptyOrNull())
            {
                var organizeId = await _userRepository.Context.Queryable<OrganizeEntity>().Where(x => x.Category == "company").Select(x => x.Id).FirstAsync();
                // 创建客户账号
                await _usersService.InnerCreate(new UserInCrInput
                {
                    id = SnowflakeIdHelper.NextId(),
                    account = phoneNumber,
                    realName = phoneNumber,
                    password = CommonConst.DEFAULTPASSWORD,
                    mobilePhone = phoneNumber,
                    roleId = config.defaultRoleId,
                    organizeId = organizeId, // 获取集团账号
                    origin = 1,
                    sid = "", // 登录成功才更新 input.sid ?? "",
                    expireTime = DateTime.Now.AddDays(Math.Max(config.defaultExperienceDays, 1))
                });
            }
            else
            {
                throw Oops.Oh("账号不存在！");
            }
        }
        #endregion

        // 根据用户账号获取用户秘钥
        var users = await _sqlSugarClient.Queryable<UserEntity>().Where(it => it.MobilePhone.Equals(input.account) && it.DeleteMark == null).Take(2).ToListAsync();
        if (users.IsAny() && users.Count > 1)
        {
            throw Oops.Oh("手机号码绑定多个账号，不允许登录！");
        }
        var user = users.FirstOrDefault();
        _ = user ?? throw Oops.Oh(ErrorCode.D5002);

        // 验证账号是否未被激活
        if (user.EnabledMark == null) throw Oops.Oh(ErrorCode.D1018);

        // 验证账号是否被禁用
        if (user.EnabledMark == 0) throw Oops.Oh(ErrorCode.D1019);

        // 验证账号是否被删除
        if (user.DeleteMark == 1) throw Oops.Oh(ErrorCode.D1017);

        // 读取配置文件
        var array = new Dictionary<string, string>();
        var sysConfigData = await _sqlSugarClient.Queryable<SysConfigEntity>().Where(x => x.Category.Equals("SysConfig")).ToListAsync();
        foreach (var item in sysConfigData)
        {
            if (!array.ContainsKey(item.Key)) array.Add(item.Key, item.Value);
        }

        var sysConfig = array.ToObject<SysConfigByOAuthModel>();

        // 是否延迟登录
        if (sysConfig.lockType.Equals(ErrorStrategy.Delay) && user.UnLockTime.IsNullOrEmpty())
        {
            if (user.UnLockTime > DateTime.Now)
            {
                int unlockTime = ((user.UnLockTime - DateTime.Now)?.TotalMinutes).ParseToInt();
                if (unlockTime < 1) unlockTime = 1;
                throw Oops.Oh(ErrorCode.D1027, unlockTime);
            }
            else if (user.UnLockTime <= DateTime.Now)
            {
                user.EnabledMark = 1;
                user.LogErrorCount = 0;
                await _sqlSugarClient.Updateable(user).UpdateColumns(it => new { it.LogErrorCount, it.EnabledMark }).ExecuteCommandAsync();
            }
        }

        // 是否 延迟登录
        if (sysConfig.lockType.Equals(ErrorStrategy.Delay) && user.UnLockTime.IsNotEmptyOrNull() && user.UnLockTime > DateTime.Now)
        {
            int? t3 = (user.UnLockTime - DateTime.Now)?.TotalMinutes.ParseToInt();
            if (t3 < 1) t3 = 1;
            throw Oops.Oh(ErrorCode.D1027, t3?.ToString());
        }

        if (sysConfig.lockType.Equals(ErrorStrategy.Delay) && user.UnLockTime.IsNotEmptyOrNull() && user.UnLockTime <= DateTime.Now)
        {
            user.EnabledMark = 1;
            user.LogErrorCount = 0;
            await _sqlSugarClient.Updateable(user).UpdateColumns(it => new { it.LogErrorCount, it.EnabledMark }).ExecuteCommandAsync();
        }

        // 是否锁定
        if (user.EnabledMark == 2) throw Oops.Oh(ErrorCode.D1031);

        // 手机登录需要验证码
        if (string.IsNullOrEmpty(input.code))
        {
            throw Oops.Oh("请输入验证码.");
        }

        if (!string.IsNullOrEmpty(input.code))
        {
            if (string.IsNullOrWhiteSpace(input.timestamp) || string.IsNullOrWhiteSpace(input.code))
                throw Oops.Oh(ErrorCode.D1029);
            string imageCode = await GetCode(input.timestamp);
            if (imageCode.IsNullOrEmpty())
                throw Oops.Oh(ErrorCode.D1030);
            if (!input.code.ToLower().Equals(imageCode.ToLower()))
                throw Oops.Oh(ErrorCode.D1029);
        }


        // app权限验证
        if (UserAgent.isMobileBrowser && user.IsAdministrator == 0 && !ExistRoleByApp(user.RoleId))
            throw Oops.Oh(ErrorCode.D1022);

        // 登录成功时 判断单点登录信息
        int whitelistSwitch = Convert.ToInt32(sysConfig.whitelistSwitch);
        string whiteListIp = sysConfig.whiteListIp;
        if (whitelistSwitch.Equals(1) && user.IsAdministrator.Equals(0))
        {
            if (!whiteListIp.Split(",").Contains(NetHelper.Ip))
                throw Oops.Oh(ErrorCode.D9002);
        }

        // token过期时间
        long tokenTimeout = sysConfig.tokenTimeout;

        // 绑定默认角色，
        // 绑定规则：
        // 0、只有一个角色的情况才默认绑定
        // 1、lastRoleId没有值 或者 lastRoleId 不在 roleid 里面
        bool switchRole = false; // 是否需要选择角色
        var roles = user.RoleId?.Split(",", StringSplitOptions.RemoveEmptyEntries)?.ToList() ?? new List<string>();
        // 判断角色是否有授权
        if (roles.IsAny() && roles.Count > 1)
        {
            var items = await _sqlSugarClient.Context.Queryable<AuthorizeEntity>()
                   .In(a => a.ObjectId, roles)
                   .Where(a => a.ItemType == "module")
                   .Where(a => SqlFunc.Subqueryable<ModuleEntity>().Where(d => d.Id == a.ItemId && d.EnabledMark == 1 && d.DeleteMark == null).Any())
                   .ToListAsync();

            // 获取禁用的权限
            var disableItems = await _sqlSugarClient.Context.Queryable<AuthorizeDisableEntity>()
                .In(a => a.ObjectId, roles)
                .Where(a => a.ItemType == "module")
                .Where(a => SqlFunc.Subqueryable<ModuleEntity>().Where(d => d.Id == a.ItemId && d.EnabledMark == 1 && d.DeleteMark == null).Any())
                .ToListAsync();

            // 排除掉禁用的权限
            if (disableItems.IsAny())
            {
                items = items.Where(it => !disableItems.Any(x => x.ObjectId == it.ObjectId && x.ItemId == it.ItemId)).ToList();
            }

            roles = items.Select(it => it.ObjectId).Distinct().ToList();
        }
        if (!string.IsNullOrEmpty(user.LastRoleId) && (roles.Count == 0 || !roles.Contains(user.LastRoleId)))
        {
            user.LastRoleId = string.Empty;
        }
        if (string.IsNullOrEmpty(user.LastRoleId) && roles.Count > 0)
        {
            // 如果多角色的普通用户没有登录过系统（LastRoleId 为空）,需要选择角色
            switchRole = user.IsAdministrator.Equals(0) && roles.Count > 1;
            user.LastRoleId = roles[0];
        }
        await _sqlSugarClient.Context.Updateable<UserEntity>(user).UpdateColumns(it => it.LastRoleId).ExecuteCommandAsync();

        // 更新推荐人， 有sid,且sid不是本人
        if (input.sid.IsNotEmptyOrNull() && user.Sid.IsNullOrEmpty() && input.sid != user.Id)
        {
            // 判断sid是否本人的下级
            var childrens = await _userRepository.AsQueryable().ToChildListAsync(it => it.Sid, user.Id);
            if (!childrens.IsAny() || !childrens.Any(x => x.Id == input.sid))
            {
                List<string> updateColumns = new List<string>()
                {
                    nameof(UserEntity.Sid)
                };
                user.Sid = input.sid;
                if (user.ManagerId.IsNullOrEmpty())
                {
                    user.ManagerId = input.sid;
                    updateColumns.Add(nameof(UserEntity.ManagerId));
                }
                await _sqlSugarClient.Context.Updateable<UserEntity>(user).UpdateColumns(updateColumns.ToArray()).ExecuteCommandAsync();
            }
        }
        // 判断是否有绑定公司，默认选择第一个
        //获取用户绑定的公司
        var olist = await _usersService.GetRelationOrganizeList(user.Id);
        if (olist != null && olist.Any())
        {
            string cacheKey = string.Format("{0}{1}_{2}", CommonConst.CACHEKEYONLINEUSER, user.Id, "company");
            var org = await _cacheManager.GetAsync<OrganizeEntity>(cacheKey);
            // 不存在才设置第一个
            if (org == null || !olist.Any(x => x.Id == org.Id))
            {
                await _cacheManager.SetAsync(cacheKey, olist[0]);
            }
        }

        // 生成Token令牌
        string accessToken = JWTEncryption.Encrypt(
                new Dictionary<string, object>
                {
                    { ClaimConst.CLAINMUSERID, user.Id },
                    { ClaimConst.CLAINMACCOUNT, user.Account },
                    { ClaimConst.CLAINMREALNAME, user.RealName },
                    { ClaimConst.CLAINMADMINISTRATOR, user.IsAdministrator },
                    { ClaimConst.TENANTID, tenantId },
                    { ClaimConst.TENANTDBNAME, tenantDbName },
                    { ClaimConst.SINGLELOGIN, (int)sysConfig.singleLogin },
                    //{ ClaimConst.CLAINMCOMPANYID, companyId },
                    //{ ClaimConst.CLAINM_JT_COMPANY_ACCOUNT, isJT?1:0 }
                }, tokenTimeout);

        // 设置Swagger自动登录
        _httpContextAccessor.HttpContext.SigninToSwagger(accessToken);

        // 设置刷新Token令牌
        _httpContextAccessor.HttpContext.Response.Headers["x-access-token"] = JWTEncryption.GenerateRefreshToken(accessToken, 30); // 生成刷新Token令牌

        var ip = NetHelper.Ip;

        // 修改用户登录信息
        await _eventPublisher.PublishAsync(new UserEventSource("User:UpdateUserLogin", tenantId, tenantDbName, new UserEntity
        {
            Id = user.Id,
            FirstLogIP = user.FirstLogIP ?? ip,
            FirstLogTime = user.FirstLogTime ?? DateTime.Now,
            PrevLogTime = user.LastLogTime,
            PrevLogIP = user.LastLogIP,
            LastLogTime = DateTime.Now,
            LastLogIP = ip,
            LogSuccessCount = user.LogSuccessCount + 1
        }));

        // 增加登录日志
        await _eventPublisher.PublishAsync(new LogEventSource("Log:CreateVisLog", tenantId, tenantDbName, new SysLogEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            UserId = user.Id,
            UserName = user.RealName,
            Category = 1,
            IPAddress = ip,
            Abstracts = "登录成功",
            PlatForm = string.Format("{0}-{1}", UserAgent.GetSystem(), UserAgent.GetBrowser()),
            CreatorTime = DateTime.Now
        }));

        return new
        {
            theme = user.Theme == null ? "classic" : user.Theme,
            token = string.Format("Bearer {0}", accessToken),
            switchRole = switchRole
        };
    }


    /// <summary>
    /// 解密.
    /// </summary>
    [HttpGet("Decrypt")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task<dynamic> Decrypt([FromQuery, Required] string secret)
    {
        string str = string.Empty;
        // 判断是否需要查数据库
        if (secret.IndexOf('@') > -1)
        {
            var index = secret.IndexOf('@');
            var tenant = secret.Substring(0, index);
            var id = secret.Substring(index + 1);

            await TenantScoped.Create(tenant, async (factory, scope) =>
            {
                var rep = scope.ServiceProvider.GetService<ISqlSugarRepository<WechatSceneEntity>>();
                var entity = await rep.SingleAsync(x => x.Id == id);
                if (entity != null)
                {
                    str = entity.Content;
                }
            });
        }

        if (str.IsNullOrEmpty())
        {
            try
            {
                str = DESCEncryption.Decrypt(secret, "QT");
            }
            catch (Exception)
            {
            }
        }



        if (str.IsNullOrEmpty())
        {
            str = "{}";
        }

        //// 判断是否需要查数据库
        //if (str.IndexOf('@')>-1)
        //{
        //    var index = str.IndexOf('@');
        //    var tenant = str.Substring(0, index);
        //    var id = str.Substring(index + 1);

        //    await TenantScoped.Create(tenant, async (factory, scope) =>
        //    {
        //        var rep = scope.ServiceProvider.GetService<ISqlSugarRepository<WechatSceneEntity>>();
        //        var entity = await rep.SingleAsync(x => x.Id == id);
        //        if (entity != null)
        //        {
        //            str = entity.Content;
        //        }
        //    });
        //}

        return JObject.Parse(str);
    }

    #region 微信相关接口

    /// <summary>
    /// 根据code 获取手机号码
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpGet("wx/login/phonenumber")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task<dynamic> WechatLoginByPhonenumber([FromQuery, Required] string code, string sid, [FromServices] IWechatAppProxy wechatAppProxy)
    {
        try
        {
            var response = await wechatAppProxy.GetUserPhonenumber(new WxGetUserPhonenumberRequest { code = code });
            // 请求成功
            if (response.errcode == 0)
            {
                var phoneNumber = response.phone_info.purePhoneNumber;
                // 判断账号是否存在
                if (!await _userRepository.AnyAsync(x => x.MobilePhone == phoneNumber))
                {
                    var config = await _coreSysConfigService.GetConfig<IotConfigs>();
                    //var config = await _userRepository.Context.Queryable<SysConfigEntity>().Where(x => x.Key == nameof(SysConfigOutput.defaultRoleId)).FirstAsync();
                    if (config != null && config.isUseDemoRole && config.defaultRoleId.IsNotEmptyOrNull())
                    {
                        var organizeId = await _userRepository.Context.Queryable<OrganizeEntity>().Where(x => x.Category == "company").Select(x => x.Id).FirstAsync();
                        // 创建客户账号
                        await _usersService.InnerCreate(new UserInCrInput
                        {
                            id = SnowflakeIdHelper.NextId(),
                            account = phoneNumber,
                            realName = phoneNumber,
                            password = CommonConst.DEFAULTPASSWORD,
                            mobilePhone = phoneNumber,
                            roleId = config.defaultRoleId,
                            organizeId = organizeId, // 获取集团账号
                            origin = 1,
                            sid = sid ?? "",
                            expireTime = DateTime.Now.AddDays(Math.Max(config.defaultExperienceDays, 1))
                        });
                    }
                }

                var timestamp = Guid.NewGuid().ToString("N");
                await _cacheManager.SetAsync(string.Format("{0}{1}", CommonConst.CACHEKEYCODE, timestamp), code, TimeSpan.FromMinutes(5));

                return await LoginByPhoneNumber(new LoginInput
                {
                    account = phoneNumber,
                    code = code,
                    timestamp = timestamp
                });
            }
        }
        catch (Exception ex)
        {
            UnifyContext.Fill(ex.StackTrace);
            throw ex;
        }

        throw Oops.Oh("登录失败！");
    }
    #endregion

    #region PrivateMethod

    /// <summary>
    /// 获取验证码.
    /// </summary>
    /// <param name="timestamp">时间戳.</param>
    /// <returns></returns>
    private async Task<string> GetCode(string timestamp)
    {
        string cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYCODE, timestamp);
        var code = await App.GetService<ICache>().GetAsync<string>(cacheKey);
        if (code.IsNullOrEmpty())
        {
            code = await _cacheManager.GetAsync<string>(cacheKey);
        }
        return code;
    }

    /// <summary>
    /// 判断app用户角色是否存在且有效.
    /// </summary>
    /// <param name="roleIds"></param>
    /// <returns></returns>
    private bool ExistRoleByApp(string roleIds)
    {
        if (roleIds.IsEmpty())
            return false;
        var roleIdList1 = roleIds.Split(",").ToList();
        var roleIdList2 = _sqlSugarClient.Queryable<RoleEntity>().Where(x => x.DeleteMark == null && x.EnabledMark == 1).Select(x => x.Id).ToList();
        return roleIdList1.Intersect(roleIdList2).ToList().Count > 0;
    }

    /// <summary>
    /// 记录密码错误次数.
    /// </summary>
    /// <param name="entity">用户实体.</param>
    /// <param name="sysConfigOutput">系统配置输出.</param>
    /// <returns></returns>
    private async Task UpdateErrorLog(UserEntity entity, SysConfigByOAuthModel sysConfigOutput)
    {
        if (entity != null)
        {
            if (entity.EnabledMark.Equals(1) && !entity.Account.ToLower().Equals(CommonConst.SUPPER_ADMIN_ACCOUNT) && sysConfigOutput.lockType > 0 && sysConfigOutput.passwordErrorsNumber > 2)
            {

                switch (sysConfigOutput.lockType)
                {
                    case ErrorStrategy.Lock:
                        entity.EnabledMark = entity.LogErrorCount >= sysConfigOutput.passwordErrorsNumber - 1 ? 2 : 1;
                        break;
                    case ErrorStrategy.Delay:
                        entity.UnLockTime = entity.LogErrorCount >= sysConfigOutput.passwordErrorsNumber - 1 ? DateTime.Now.AddMinutes(sysConfigOutput.lockTime) : null;
                        entity.EnabledMark = entity.LogErrorCount >= sysConfigOutput.passwordErrorsNumber - 1 ? 2 : 1;
                        break;
                }

                entity.LogErrorCount++;

                await _sqlSugarClient.Updateable(entity).UpdateColumns(it => new { it.EnabledMark, it.UnLockTime, it.LogErrorCount }).ExecuteCommandAsync();
            }
        }
    }

    /// <summary>
    /// 获取在线用户列表.
    /// </summary>
    /// <returns></returns>
    public async Task<List<UserOnlineModel>> GetOnlineUserList()
    {
        string cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYONLINEUSER, _userManager.TenantId);
        return await _cacheManager.GetAsync<List<UserOnlineModel>>(cacheKey);
    }

    /// <summary>
    /// 保存在线用户列表.
    /// </summary>
    /// <param name="onlineList">在线用户列表.</param>
    /// <returns></returns>
    public async Task<bool> SetOnlineUserList(List<UserOnlineModel> onlineList)
    {
        string cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYONLINEUSER, _userManager.TenantId);
        return await _cacheManager.SetAsync(cacheKey, onlineList);
    }

    /// <summary>
    /// 删除用户登录信息缓存.
    /// </summary>
    public async Task<bool> DelUserInfo()
    {
        string cacheKey = string.Format("{0}{1}_{2}", CommonConst.CACHEKEYUSER, _userManager.TenantId, _userManager.UserId);
        return await _cacheManager.DelAsync(cacheKey);
    }

    /// <summary>
    /// 获取真实的公司
    /// </summary>
    /// <param name="list"></param>
    /// <param name="current"></param>
    /// <returns></returns>
    private OrganizeEntity? GetRealOrg(List<OrganizeEntity> list, OrganizeEntity? current)
    {
        if (current != null && !string.IsNullOrEmpty(current.ParentId))
        {
            var parent = list.Find(x => x.Id == current.ParentId);
            if (parent != null && list.Any(x => x.Id == parent.ParentId))
            {
                return GetRealOrg(list, parent);
            }
            else
            {
                return current;
            }
        }
        return null;
    }
    #endregion
}