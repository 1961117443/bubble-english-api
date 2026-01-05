using AI.BubbleEnglish.Dto;
using AI.BubbleEnglish.Entitys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QT.Common.Const;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logging.Attributes;
using SqlSugar;

namespace AI.BubbleEnglish.Controller;

/// <summary>
/// 家长登录（微信/手机号）
/// 说明：手机号登录主要用于开发测试；后续可替换为真实短信/运营商能力。
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleEnglish", Name = "Auth", Order = 10)]
[Route("api/bubbleEnglish/auth")]
public class AuthController : IDynamicApiController
{
    private readonly ISqlSugarRepository<BubbleUserEntity> _userRepo;
    private readonly ITenantManager _tenantManager;
    private readonly IHttpContextAccessor _http;

    public AuthController(ISqlSugarRepository<BubbleUserEntity> userRepo, ITenantManager tenantManager, IHttpContextAccessor http)
    {
        _userRepo = userRepo;
        _tenantManager = tenantManager;
        _http = http;
    }

    /// <summary>
    /// 手机号 + 密码 登录（测试用：密码不校验，未注册则自动创建账号）
    /// </summary>
    [AllowAnonymous]
    [HttpPost("phone/password")]
    [IgnoreLog]
    public async Task<AuthLoginResp> LoginByPhonePassword([FromBody] PhonePasswordLoginReq req)
    {
        if (string.IsNullOrWhiteSpace(req.Phone)) throw Oops.Oh("手机号不能为空");

        var user = await _userRepo.AsQueryable().FirstAsync(x => x.Phone == req.Phone);
        if (user == null)
        {
            user = new BubbleUserEntity
            {
                Phone = req.Phone,
                Nickname = "家长",
                AvatarUrl = string.Empty,
                OpenId = string.Empty,
                IsAdmin = 0,
                CreateTime = DateTime.Now
            };
            user.Id = await _userRepo.InsertReturnIdentityAsync(user);
        }

        return BuildLoginResp(user);
    }

    /// <summary>
    /// 获取短信验证码（开发期：固定返回 123456，仅用于联调）
    /// </summary>
    [AllowAnonymous]
    [HttpPost("phone/code/request")]
    [IgnoreLog]
    public Task<object> RequestSmsCode([FromBody] SmsCodeRequestReq req)
    {
        if (string.IsNullOrWhiteSpace(req.Phone)) throw Oops.Oh("手机号不能为空");
        return Task.FromResult<object>(new { ok = true, code = "123456" });
    }

    /// <summary>
    /// 手机号 + 验证码 登录（测试用：验证码固定 123456；未注册则自动创建账号）
    /// </summary>
    [AllowAnonymous]
    [HttpPost("phone/code/login")]
    [IgnoreLog]
    public async Task<AuthLoginResp> LoginByPhoneCode([FromBody] PhoneCodeLoginReq req)
    {
        if (string.IsNullOrWhiteSpace(req.Phone)) throw Oops.Oh("手机号不能为空");
        if (string.IsNullOrWhiteSpace(req.Code)) throw Oops.Oh("验证码不能为空");
        if (req.Code != "123456") throw Oops.Oh("验证码错误（测试阶段固定 123456）");

        var user = await _userRepo.AsQueryable().FirstAsync(x => x.Phone == req.Phone);
        if (user == null)
        {
            user = new BubbleUserEntity
            {
                Phone = req.Phone,
                Nickname = "家长",
                AvatarUrl = string.Empty,
                OpenId = string.Empty,
                IsAdmin = 0,
                CreateTime = DateTime.Now
            };
            user.Id = await _userRepo.InsertReturnIdentityAsync(user);
        }

        return BuildLoginResp(user);
    }

    /// <summary>
    /// 微信登录（占位：后续对接 wx.login code 换取 openid，再绑定手机号）
    /// </summary>
    [AllowAnonymous]
    [HttpPost("wechat")]
    [IgnoreLog]
    public Task<object> LoginByWechat([FromBody] WechatLoginReq req)
    {
        // TODO: 调用微信接口：jscode2session
        // TODO: 通过 openid 找/建 BubbleUserEntity，生成 token
        return Task.FromResult<object>(new { ok = false, message = "TODO backend: wechat login" });
    }

    private AuthLoginResp BuildLoginResp(BubbleUserEntity user)
    {
        // token过期时间（分钟）
        long tokenTimeout = 120;

        var accessToken = JWTEncryption.Encrypt(
            new Dictionary<string, object>
            {
                { ClaimConst.CLAINMUSERID, user.Id },
                { ClaimConst.CLAINMACCOUNT, user.Phone ?? user.OpenId ?? string.Empty },
                { ClaimConst.CLAINMREALNAME, string.IsNullOrWhiteSpace(user.Nickname) ? "家长" : user.Nickname },
                { ClaimConst.TENANTID, _tenantManager.TenantId },
            },
            tokenTimeout);

        // 设置Swagger自动登录（如果启用 swagger）
        _http.HttpContext?.SigninToSwagger(accessToken);

        return new AuthLoginResp
        {
            Token = accessToken,
            Parent = new ParentDto
            {
                Id = user.Id,
                Phone = user.Phone,
                OpenId = user.OpenId,
                Nickname = user.Nickname,
                AvatarUrl = user.AvatarUrl
            }
        };
    }
}
