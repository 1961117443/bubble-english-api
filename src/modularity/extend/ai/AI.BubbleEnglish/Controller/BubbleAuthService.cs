using AI.BubbleEnglish.Dto;
using Microsoft.AspNetCore.Mvc;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.OAuth;
using QT.OAuth.Dto;

namespace AI.BubbleEnglish;

/// <summary>
/// BubbleEnglish 认证入口（推荐：最终由 QT.OAuth 统一发 Token）
///
/// 说明：
/// - 账号密码登录：直接代理 QT.OAuth/OAuthService.Login
/// - 手机号验证码/微信登录：当前仅占位，建议在 QT.OAuth 扩展实现
/// </summary>
[ApiDescriptionSettings(Tag = "BubbleEnglish", Name = "Auth", Order = 1000)]
[Route("api/BubbleEnglish/[controller]")]
public class BubbleAuthService : IDynamicApiController, ITransient
{
    private readonly OAuthService _oauth;

    public BubbleAuthService(OAuthService oauth)
    {
        _oauth = oauth;
    }

    /// <summary>
    /// 手机号+密码登录（测试用，代理 OAuth/Login）
    /// </summary>
    [HttpPost("phone/password")]
    [Consumes("application/json")]
    [AllowAnonymous]
    public async Task<dynamic> PhonePasswordLogin([FromBody] PhonePasswordLoginInput input)
    {
        if (string.IsNullOrWhiteSpace(input.phone)) throw Oops.Oh("手机号不能为空");
        if (string.IsNullOrWhiteSpace(input.password)) throw Oops.Oh("密码不能为空");

        // OAuth 里账号字段叫 account，这里我们直接用手机号作为 account。
        var o = new LoginInput
        {
            account = input.phone,
            password = input.password,
            origin = "bubble_english"
        };
        return await _oauth.Login(o);
    }

    /// <summary>
    /// 请求短信验证码（占位）
    /// </summary>
    [HttpPost("phone/code/request")]
    [Consumes("application/json")]
    [AllowAnonymous]
    public async Task<dynamic> RequestPhoneCode([FromBody] PhoneCodeRequestInput input)
    {
        // 建议：在 QT.OAuth 统一实现短信验证码；此处仅提供调试占位。
        await Task.CompletedTask;
        return new { ok = true, code = "123456", msg = "调试占位：固定验证码 123456（请尽快接入短信服务）" };
    }

    /// <summary>
    /// 手机号+验证码登录（占位）
    /// </summary>
    [HttpPost("phone/code/login")]
    [Consumes("application/json")]
    [AllowAnonymous]
    public async Task<dynamic> PhoneCodeLogin([FromBody] PhoneCodeLoginInput input)
    {
        await Task.CompletedTask;
        throw Oops.Oh("手机号验证码登录建议在 QT.OAuth 扩展实现：此接口仅占位。");
    }

    /// <summary>
    /// 微信小程序 code 登录（占位）
    /// </summary>
    [HttpPost("wechat/miniapp")]
    [Consumes("application/json")]
    [AllowAnonymous]
    public async Task<dynamic> WechatMiniLogin([FromBody] WechatMiniLoginInput input)
    {
        await Task.CompletedTask;
        throw Oops.Oh("微信小程序登录建议在 QT.OAuth 扩展实现：此接口仅占位。");
    }
}
