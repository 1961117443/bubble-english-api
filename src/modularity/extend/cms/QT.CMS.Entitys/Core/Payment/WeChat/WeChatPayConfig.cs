using System;
using System.Collections.Generic;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 微信支付常量定义
/// </summary>
public static class WeChatPayConfig
{
    /// <summary>
    /// JsApi下单接口
    /// </summary>
    public const string JsApiPayUrl = "https://api.mch.weixin.qq.com/v3/pay/transactions/jsapi";
    /// <summary>
    /// APP下单接口
    /// </summary>
    public const string AppPayUrl = "https://api.mch.weixin.qq.com/v3/pay/transactions/app";
    /// <summary>
    /// H5下单接口
    /// </summary>
    public const string H5PayUrl = "https://api.mch.weixin.qq.com/v3/pay/transactions/h5";
    /// <summary>
    /// 扫码下单接口
    /// </summary>
    public const string NativePayUrl = "https://api.mch.weixin.qq.com/v3/pay/transactions/native";
    /// <summary>
    /// 退款申请接口
    /// </summary>
    public const string RefundsUrl = "https://api.mch.weixin.qq.com/v3/refund/domestic/refunds";
    /// <summary>
    /// 获取用户授权接口
    /// </summary>
    public const string JsOAuthUrl = "https://api.weixin.qq.com/sns/jscode2session";
    /// <summary>
    /// 获取Code接口
    /// </summary>
    public const string OAuth2Url = "https://open.weixin.qq.com/connect/oauth2/authorize";
    /// <summary>
    /// 获取AccessToken接口
    /// </summary>
    public const string AccessTokenUrl = "https://api.weixin.qq.com/sns/oauth2/access_token";
    /// <summary>
    /// GET 获取平台证书列表
    /// </summary>
    public const string CertificateUrl = "https://api.mch.weixin.qq.com/v3/certificates";
}
