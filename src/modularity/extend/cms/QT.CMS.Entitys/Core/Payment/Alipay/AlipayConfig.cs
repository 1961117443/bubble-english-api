using System;
using System.Collections.Generic;
using System.Text;

namespace QT.CMS.Entitys;

public static class AlipayConfig
{
    /// <summary>
    /// 支付宝网关地址
    /// </summary>
    public const string SERVER_URL= "https://openapi.alipay.com/gateway.do";

    /// <summary>
    /// 应用Id
    /// </summary>
    public const string APP_ID = "app_id";
    /// <summary>
    /// 接口名称
    /// </summary>
    public const string METHOD = "method";
    /// <summary>
    /// 数据格式，默认为："json"
    /// </summary>
    public const string FORMAT = "format";
    /// <summary>
    /// 支付成功后跳转URL
    /// HTTP/HTTPS开头字符串
    /// </summary>
    public const string RETURN_URL = "return_url";
    /// <summary>
    /// 编码格式，默认为："utf-8"
    /// </summary>
    public const string CHARSET = "charset";
    /// <summary>
    /// 签名方式，默认为："RSA2"
    /// </summary>
    public const string SIGN_TYPE = "sign_type";
    /// <summary>
    /// 商户请求参数的签名串
    /// </summary>
    public const string SIGN = "sign";
    /// <summary>
    /// 发送请求的时间，格式"yyyy-MM-dd HH:mm:ss"
    /// </summary>
    public const string TIMESTAMP = "timestamp";
    /// <summary>
    /// 接口版本，默认为："1.0"
    /// </summary>
    public const string VERSION = "version";
    /// <summary>
    /// 支付宝服务器主动通知商户服务器里指定的页面http/https路径
    /// </summary>
    public const string NOTIFY_URL = "notify_url";
    /// <summary>
    /// 第三方应用授权
    /// </summary>
    public const string APP_AUTH_TOKEN = "app_auth_token";
    /// <summary>
    /// 请求参数的集合，最大长度不限，除公共参数外所有请求参数都必须放在这个参数中传递
    /// </summary>
    public const string BIZ_CONTENT = "biz_content";
}
