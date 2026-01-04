using Microsoft.AspNetCore.Http;
using QT.CMS.Entitys.WeChat;
using QT.CMS.Utils;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.RemoteRequest.Extensions;
using QT.Systems.Interfaces.System;
using Senparc.CO2NET.Extensions;
using System.Security.Cryptography.X509Certificates;

namespace QT.CMS;

/// <summary>
/// 微信支付接口实现
/// </summary>
public class WeChatExecuteService : WeChatBase, IWeChatExecuteService,IScoped
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IQRCodeService _qRCodeService;

    public WeChatExecuteService(IHttpContextAccessor httpContextAccessor, ISqlSugarRepository<SitePayment> sitePaymentService, IQRCodeService qRCodeService) : base(sitePaymentService)
    {
        _httpContextAccessor = httpContextAccessor;
        _qRCodeService = qRCodeService;
    }

    /// <summary>
    /// APP下单支付
    /// </summary>
    public async Task<WeChatPayAppParamDto> AppPayAsync(WeChatPayDto modelDto)
    {
        //取得微信支付账户
        var wxAccount = await GetAccountAsync(modelDto.PaymentId);

        //赋值下单实体(注意附加数据为站点ID)
        WeChatPayBodyDto model = new()
        {
            AppId = wxAccount.AppId,
            MchId = wxAccount.MchId,
            Description = modelDto.Description,
            OutTradeNo = modelDto.OutTradeNo,
            NotifyUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{wxAccount.NotifyUrl}/transactions/{modelDto.PaymentId}",
            Amount = new Amount { Total = (int)(modelDto.Total * 100), Currency = "CNY" }
        };
        var url = WeChatPayConfig.AppPayUrl;
        var content = model.ToJson();
        var auth = BuildAuth(url, "POST", content, wxAccount.MchId, wxAccount.CertPath, wxAccount.MchId);
        //发送POST请求
        ICollection<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
        headers.Add(new KeyValuePair<string, string>("Authorization", $"WECHATPAY2-SHA256-RSA2048 {auth}"));
        headers.Add(new KeyValuePair<string, string>("User-Agent", "Unknown"));
        headers.Add(new KeyValuePair<string, string>("Accept", "application/json"));

        var result = await url.SetHeaders(new Dictionary<string, object>
        {
            {"Authorization", $"WECHATPAY2-SHA256-RSA2048 {auth}" },
            {"User-Agent", "Unknown" },
            {"Accept", "application/json" }
        })
            .SetBody(content)
            .PostAsync();
        var dic = result.ToObject<Dictionary<string, string>>();

        var statusCode =(int) result.StatusCode;
        var reBody = await result.Content.ReadAsStringAsync();
        //var (statusCode, reHeaders, reBody) = await RequestHelper.PostAsync(url, headers, content);
        if (statusCode != 200)
        {
            throw Oops.Oh($"微信下单失败，错误代码：{statusCode},{reBody}");
        }
        var prepayId = reBody.ToObject<WeChatPayAppResultDto>()?.PrepayId;
        //var prepayId = JsonHelper.ToJson<WeChatPayAppResultDto>(reBody)?.PrepayId;
        return BuildAppParam(wxAccount.AppId, wxAccount.MchId, prepayId, wxAccount.CertPath, wxAccount.MchId);
    }

    /// <summary>
    /// JSAPI下单支付
    /// </summary>
    public async Task<WeChatPayJsApiParamDto> JsApiPayAsync(WeChatPayDto modelDto, bool mp = true)
    {
        //检查是否有Code
        if (!modelDto.Code.IsNotEmptyOrNull())
        {
            throw Oops.Oh($"无法获取用户OpenId所需的Code");
        }
        //取得微信支付账户
        var wxAccount = await GetAccountAsync(modelDto.PaymentId);
        //获取微信用户OPENID
        var openId = string.Empty;
        if (mp)
        {
            openId = await JsOAuthOpenId(wxAccount.AppId, wxAccount.AppSecret, modelDto.Code);
        }
        else
        {
            openId = await OAuth2OpenId(wxAccount.AppId, wxAccount.AppSecret, modelDto.Code);
        }


        //赋值下单实体(注意附加数据为站点ID)
        WeChatPayBodyDto model = new WeChatPayBodyDto
        {
            MchId = wxAccount.MchId,
            OutTradeNo = modelDto.OutTradeNo,
            AppId = wxAccount.AppId,
            Description = modelDto.Description,
            NotifyUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{wxAccount.NotifyUrl}/transactions/{modelDto.PaymentId}",
            Amount = new Amount { Total = (int)(modelDto.Total * 100), Currency = "CNY" },
            Payer = new PayerInfo { OpenId = openId }
        };
        var url = WeChatPayConfig.JsApiPayUrl;
        var content = model.ToJson();
        var auth = BuildAuth(url, "POST", content, wxAccount.MchId, wxAccount.CertPath, wxAccount.MchId);
        //发送POST请求
        ICollection<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
        headers.Add(new KeyValuePair<string, string>("Authorization", $"WECHATPAY2-SHA256-RSA2048 {auth}"));
        headers.Add(new KeyValuePair<string, string>("User-Agent", "Unknown"));
        headers.Add(new KeyValuePair<string, string>("Accept", "application/json"));

        var result = await url.SetHeaders(new Dictionary<string, object>
        {
            {"Authorization", $"WECHATPAY2-SHA256-RSA2048 {auth}" },
            {"User-Agent", "Unknown" },
            {"Accept", "application/json" }
        })
            .SetBody(content)
            .PostAsync();
        var dic = result.ToObject<Dictionary<string, string>>();

        var statusCode = (int)result.StatusCode;
        var reBody = await result.Content.ReadAsStringAsync();

        //var (statusCode, reHeaders, reBody) = await RequestHelper.PostAsync(url, headers, content);
        if (statusCode != 200)
        {
            throw Oops.Oh($"微信下单失败，错误代码：{statusCode},{reBody}");
        }
        var prepayId = reBody.ToObject<WeChatPayJsApiResultDto>()?.PrepayId;
        //var prepayId = JsonHelper.ToJson<WeChatPayJsApiResultDto>(reBody)?.PrepayId;
        return BuildJsApiParam(wxAccount.AppId, prepayId, wxAccount.CertPath, wxAccount.MchId);
    }

    /// <summary>
    /// H5下单支付
    /// </summary>
    public async Task<WeChatPayH5ParamDto> H5PayAsync(WeChatPayDto modelDto)
    {
        //取得微信支付账户
        var wxAccount = await GetAccountAsync(modelDto.PaymentId);

        //赋值下单实体(注意附加数据为站点ID)
        WeChatPayBodyDto model = new WeChatPayBodyDto
        {
            MchId = wxAccount.MchId,
            OutTradeNo = modelDto.OutTradeNo,
            AppId = wxAccount.AppId,
            Description = modelDto.Description,
            NotifyUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{wxAccount.NotifyUrl}/transactions/{modelDto.PaymentId}",
            Amount = new Amount { Total = (int)(modelDto.Total * 100), Currency = "CNY" },
            SceneInfo = new SceneInfo
            {
                PayerClientIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString(),
                H5Info = new H5Info
                {
                    Type = "Wap"
                }
            }
        };
        var url = WeChatPayConfig.H5PayUrl;
        var content = model.ToJson();
        var auth = BuildAuth(url, "POST", content, wxAccount.MchId, wxAccount.CertPath, wxAccount.MchId);
        //发送POST请求
        ICollection<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
        headers.Add(new KeyValuePair<string, string>("Authorization", $"WECHATPAY2-SHA256-RSA2048 {auth}"));
        headers.Add(new KeyValuePair<string, string>("User-Agent", "Unknown"));
        headers.Add(new KeyValuePair<string, string>("Accept", "application/json"));

        var result = await url.SetHeaders(new Dictionary<string, object>
        {
            {"Authorization", $"WECHATPAY2-SHA256-RSA2048 {auth}" },
            {"User-Agent", "Unknown" },
            {"Accept", "application/json" }
        })
         .SetBody(content)
         .PostAsync();
        var dic = result.ToObject<Dictionary<string, string>>();

        var statusCode = (int)result.StatusCode;
        var reBody = await result.Content.ReadAsStringAsync();

        //var (statusCode, reHeaders, reBody) = await RequestHelper.PostAsync(url, headers, content);
        if (statusCode != 200)
        {
            throw Oops.Oh($"微信下单失败，错误代码：{statusCode},{reBody}");
        }
        var h5url = reBody.ToObject<WeChatPayH5ResultDto>()?.H5Url;
        //var h5url = JsonHelper.ToJson<WeChatPayH5ResultDto>(reBody)?.H5Url;
        if (modelDto.ReturnUri.IsNotEmptyOrNull())
        {
            h5url += $"&redirect_url={modelDto.ReturnUri}";
        }
        return new WeChatPayH5ParamDto { Url = h5url };
    }

    /// <summary>
    /// 扫码下单支付
    /// </summary>
    public async Task<WeChatPayNativeParamDto> NativePayAsync(WeChatPayDto modelDto)
    {
        //取得微信支付账户
        var wxAccount = await GetAccountAsync(modelDto.PaymentId);

        //赋值下单实体(注意附加数据为站点ID)
        WeChatPayBodyDto model = new WeChatPayBodyDto
        {
            AppId = wxAccount.AppId,
            MchId = wxAccount.MchId,
            Description = modelDto.Description,
            OutTradeNo = modelDto.OutTradeNo,
            NotifyUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{wxAccount.NotifyUrl}/transactions/{modelDto.PaymentId}",
            Amount = new Amount { Total = (int)(modelDto.Total * 100), Currency = "CNY" }
        };
        var url = WeChatPayConfig.NativePayUrl;
        var content = model.ToJson();
        var auth = BuildAuth(url, "POST", content, wxAccount.MchId, wxAccount.CertPath, wxAccount.MchId);
        //发送POST请求
        ICollection<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
        headers.Add(new KeyValuePair<string, string>("Authorization", $"WECHATPAY2-SHA256-RSA2048 {auth}"));
        headers.Add(new KeyValuePair<string, string>("User-Agent", "Unknown"));
        headers.Add(new KeyValuePair<string, string>("Accept", "application/json"));

        var result = await url.SetHeaders(new Dictionary<string, object>
        {
            {"Authorization", $"WECHATPAY2-SHA256-RSA2048 {auth}" },
            {"User-Agent", "Unknown" },
            {"Accept", "application/json" }
        })
        .SetBody(content)
        .PostAsync();
        var dic = result.ToObject<Dictionary<string, string>>();

        var statusCode = (int)result.StatusCode;
        var reBody = await result.Content.ReadAsStringAsync();


        //var (statusCode, reHeaders, reBody) = await RequestHelper.PostAsync(url, headers, content);
        if (statusCode != 200)
        {
            throw Oops.Oh($"微信下单失败，错误代码：{statusCode},{reBody}");
        }
        var codeUrl = reBody.ToObject<WeChatPayNativeResultDto>()?.CodeUrl;

        //var codeUrl = JsonHelper.ToJson<WeChatPayNativeResultDto>(reBody)?.CodeUrl;
        if (codeUrl == null)
        {
            throw Oops.Oh($"无法获取二维码地址");
        }
        //生成Base64图片
        var codeData = _qRCodeService.GenerateToString(codeUrl);
        return new WeChatPayNativeParamDto { CodeData = codeData };
    }

    /// <summary>
    /// 拼接OAuth2的URI
    /// </summary>
    public async Task<string> OAuthAsync(WeChatPayOAuthDto modelDto)
    {
        //取得微信支付账户
        var wxAccount = await GetAccountAsync(modelDto.PaymentId);
        //拼接地址
        var url = WeChatPayConfig.OAuth2Url
            + $"?appid={wxAccount.AppId}&redirect_uri={modelDto.RedirectUri}&response_type=code&scope=snsapi_base&state={modelDto.OutTradeNo}#wechat_redirect";
        return url;
    }

    /// <summary>
    /// 申请退款
    /// </summary>
    public async Task<bool> RefundAsync(WeChatPayRefundDto modelDto)
    {
        //取得微信支付账户
        var wxAccount = await GetAccountAsync(modelDto.PaymentId);
        //赋值退款参数实体
        WeChatPayRefundBodyDto model = new()
        {
            OutTradeNo = modelDto.OutTradeNo,
            OutRefundNo = modelDto.OutRefundId.ToString(),
            Reason = modelDto.Reason,
            Amount = new RefundBodyAmount
            {
                Refund = (int)(modelDto.Refund * 100),
                Total = (int)(modelDto.Total * 100),
                Currency = "CNY"
            }
        };
        var url = WeChatPayConfig.RefundsUrl;
        var content = model.ToJson();
        var auth = BuildAuth(url, "POST", content, wxAccount.MchId, wxAccount.CertPath, wxAccount.MchId);
        //发送POST请求
        ICollection<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
        headers.Add(new KeyValuePair<string, string>("Authorization", $"WECHATPAY2-SHA256-RSA2048 {auth}"));
        headers.Add(new KeyValuePair<string, string>("User-Agent", "Unknown"));
        headers.Add(new KeyValuePair<string, string>("Accept", "application/json"));


        var response = await url.SetHeaders(new Dictionary<string, object>
        {
            {"Authorization", $"WECHATPAY2-SHA256-RSA2048 {auth}" },
            {"User-Agent", "Unknown" },
            {"Accept", "application/json" }
        })
      .SetBody(content)
      .PostAsync();
        var statusCode = (int)response.StatusCode;
        var reBody = await response.Content.ReadAsStringAsync(); 

        //var (statusCode, reHeaders, reBody) = await RequestHelper.PostAsync(url, headers, content);
        if (statusCode != 200)
        {
            throw Oops.Oh($"微信申请退款失败，错误代码：{statusCode},{reBody}");
        }
        var result = reBody.ToObject<WeChatPayRefundResultDto>();
        //var result = JsonHelper.ToJson<WeChatPayRefundResultDto>(reBody);
        if (result != null && result.Status != null && (result.Status.Equals("SUCCESS") || result.Status.Equals("PROCESSING")))
        {
            return true;
        }

        return false;
    }

    #region 私有辅助方法
    /// <summary>
    /// 获取JSCODE微信用户OPENID
    /// </summary>
    private async Task<string> JsOAuthOpenId(string? appId, string? appSecret, string? jsCode)
    {
        string url = $"{WeChatPayConfig.JsOAuthUrl}?appid={appId}&secret={appSecret}&js_code={jsCode}";
        var result = await url.GetAsStringAsync();
        var dic = result.ToObject<Dictionary<string, string>>();
        //var result = await RequestHelper.GetAsync(url);
        //var dic = JsonHelper.ToJson<Dictionary<string, string>>(result);
        if (dic != null && dic.ContainsKey("openid"))
        {
            return dic["openid"];
        }
        throw Oops.Oh($"无法获取OpenID，应填写小程序的AppID和AppSecret");
    }

    /// <summary>
    /// 获取OAuth2微信用户OPENID
    /// </summary>
    private async Task<string> OAuth2OpenId(string? appId, string? appSecret, string? code)
    {
        string url = $"{WeChatPayConfig.AccessTokenUrl}?appid={appId}&secret={appSecret}&code={code}&grant_type=authorization_code";
        var result = await url.GetAsStringAsync();
        var dic = result.ToObject<Dictionary<string, string>>();
        //var result = await RequestHelper.GetAsync(url);
        //var dic = JsonHelper.ToJson<Dictionary<string, string>>(result);
        if (dic != null && dic.ContainsKey("openid"))
        {
            return dic["openid"];
        }
        throw Oops.Oh($"获取OpenId出错：{code}，{result}");
    }

    /// <summary>
    /// 生成头部Authorization
    /// </summary>
    private string BuildAuth(string url, string method, string? body, string? mchId, string? certPath, string? certPwd)
    {
        if (certPath == null)
        {
            throw Oops.Oh($"证书路径有误，请检查后重试");
        }
        var uri = new Uri(url).PathAndQuery;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var nonce = Guid.NewGuid().ToString("N");
        var message = $"{method}\n{uri}\n{timestamp}\n{nonce}\n{body}\n";
        var certificate2 = new X509Certificate2(certPath, certPwd, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
        var signature = SHA256WithRSA.Sign(certificate2.GetRSAPrivateKey(), message);
        return $"mchid=\"{mchId}\",nonce_str=\"{nonce}\",timestamp=\"{timestamp}\",serial_no=\"{certificate2.GetSerialNumberString()}\",signature=\"{signature}\"";
    }

    /// <summary>
    /// 生成JsApi下单返回参数
    /// </summary>
    private WeChatPayJsApiParamDto BuildJsApiParam(string? appId, string? prepayId, string? certPath, string? certPwd)
    {
        if (certPath == null)
        {
            throw Oops.Oh($"证书路径有误，请检查后重试");
        }
        var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var nonceStr = Guid.NewGuid().ToString("N");
        var package = $"prepay_id={prepayId}";
        string message = $"{appId}\n{timeStamp}\n{nonceStr}\n{package}\n";
        var certificate2 = new X509Certificate2(certPath, certPwd, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
        var signature = SHA256WithRSA.Sign(certificate2.GetRSAPrivateKey(), message);
        var result = new WeChatPayJsApiParamDto()
        {
            AppId = appId,
            TimeStamp = timeStamp,
            NonceStr = nonceStr,
            Package = package,
            PaySign = signature
        };
        return result;
    }

    /// <summary>
    /// 生成APP返回下单参数
    /// </summary>
    private WeChatPayAppParamDto BuildAppParam(string? appId, string? mchId, string? prepayId, string? certPath, string? certPwd)
    {
        if (certPath == null)
        {
            throw Oops.Oh($"证书路径有误，请检查后重试");
        }
        var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var nonceStr = Guid.NewGuid().ToString("N");
        var package = "Sign=WXPay";
        string message = $"{appId}\n{timeStamp}\n{nonceStr}\n{prepayId}\n";
        var certificate2 = new X509Certificate2(certPath, certPwd, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
        var signature = SHA256WithRSA.Sign(certificate2.GetRSAPrivateKey(), message);
        var result = new WeChatPayAppParamDto()
        {
            AppId = appId,
            PartnerId = mchId,
            PrepayId = prepayId,
            Package = package,
            NonceStr = nonceStr,
            TimeStamp = timeStamp,
            Sign = signature
        };
        return result;
    }
    #endregion
}
