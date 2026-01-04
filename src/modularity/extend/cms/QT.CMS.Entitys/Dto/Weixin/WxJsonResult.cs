using Newtonsoft.Json;

namespace QT.CMS.Entitys.Dto.Weixin;

/// <summary>
/// 微信返回状态
/// </summary>
public class WxJsonResult
{
    /// <summary>
    /// 返回码
    /// </summary>
    [JsonProperty("errcode")]
    public int ErrCode { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    [JsonProperty("errmsg")]
    public string? ErrMsg { get; set; }
}

/// <summary>
/// 获取AccessToken返回结果
/// </summary>
public class WxAccessTokenResult : WxJsonResult
{
    /// <summary>
    /// 获取到的凭证
    /// </summary>
    [JsonProperty("access_token")]
    public string? AccessToken { get; set; }

    /// <summary>
    /// 凭证有效时间，单位：秒
    /// </summary>
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
}

/// <summary>
/// 微信验证返回结果
/// </summary>
public class WxCheckResult
{
    /// <summary>
    /// 微信加密签名
    /// signature结合了开发者填写的 token 参数和请求中的 timestamp 参数、nonce参数
    /// </summary>
    public string? Signature { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public string? Timestamp { get; set; }

    /// <summary>
    /// 随机数
    /// </summary>
    public string? Nonce { get; set; }

    /// <summary>
    /// 随机字符串
    /// </summary>
    public string? Echostr { get; set; }
}
