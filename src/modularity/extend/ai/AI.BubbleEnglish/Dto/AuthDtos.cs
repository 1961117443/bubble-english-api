namespace AI.BubbleEnglish.Dto;

using QT.DependencyInjection;

/// <summary>
/// 手机号+密码登录（测试用）
/// </summary>
[SuppressSniffer]
public class PhonePasswordLoginInput
{
    public string phone { get; set; }
    public string password { get; set; }
}

/// <summary>
/// 请求短信验证码（占位）
/// </summary>
[SuppressSniffer]
public class PhoneCodeRequestInput
{
    public string phone { get; set; }
}

/// <summary>
/// 手机号+验证码登录（占位）
/// </summary>
[SuppressSniffer]
public class PhoneCodeLoginInput
{
    public string phone { get; set; }
    public string code { get; set; }
}

/// <summary>
/// 小程序 wx.login code 登录（占位）
/// </summary>
[SuppressSniffer]
public class WechatMiniLoginInput
{
    public string code { get; set; }
}
