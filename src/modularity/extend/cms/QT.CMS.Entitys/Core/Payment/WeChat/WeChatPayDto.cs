using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys;

#region 微信支付API返回实体
/// <summary>
/// App下单返回结果
/// </summary>
public class WeChatPayAppResultDto
{
    /// <summary>
    /// 调用JSAPI下单后返回参数
    /// </summary>
    [JsonProperty("prepay_id")]
    public string? PrepayId { get; set; }
}

/// <summary>
/// JSAPI下单返回结果
/// </summary>
public class WeChatPayJsApiResultDto
{
    /// <summary>
    /// 调用JSAPI下单后返回参数
    /// </summary>
    [JsonProperty("prepay_id")]
    public string? PrepayId { get; set; }
}

/// <summary>
/// H5下单返回结果
/// </summary>
public class WeChatPayH5ResultDto
{
    /// <summary>
    /// 调用H5下单后返回参数
    /// </summary>
    [JsonProperty("h5_url")]
    public string? H5Url { get; set; }
}

/// <summary>
/// 扫码下单返回结果
/// </summary>
public class WeChatPayNativeResultDto
{
    /// <summary>
    /// 此URL用于生成支付二维码，然后提供给用户扫码支付
    /// </summary>
    [JsonProperty("code_url")]
    public string? CodeUrl { get; set; }
}

/// <summary>
/// 申请退款返回结果
/// </summary>
public class WeChatPayRefundResultDto
{
    /// <summary>
    /// 微信支付退款单号
    /// </summary>
    [JsonProperty("refund_id")]
    public string? RefundId { get; set; }

    /// <summary>
    /// 商户退款单号
    /// </summary>
    [JsonProperty("out_refund_no")]
    public string? OutRefundNo { get; set; }

    /// <summary>
    /// 微信支付订单号
    /// </summary>
    [JsonProperty("transaction_id")]
    public string? TransactionId { get; set; }

    /// <summary>
    /// 商户订单号
    /// </summary>
    [JsonProperty("out_trade_no")]
    public string? OutTradeNo { get; set; }

    /// <summary>
    /// 退款渠道
    /// </summary>
    [JsonProperty("channel")]
    public string? Channel { get; set; }

    /// <summary>
    /// 退款入账账户
    /// </summary>
    [JsonProperty("user_received_account")]
    public string? UserReceivedAccount { get; set; }

    /// <summary>
    /// 退款成功时间
    /// </summary>
    [JsonProperty("success_time")]
    public string? SuccessTime { get; set; }

    /// <summary>
    /// 退款创建时间
    /// </summary>
    [JsonProperty("create_time")]
    public string? CreateTime { get; set; }

    /// <summary>
    /// 退款状态
    /// SUCCESS：退款成功
    /// CLOSED：退款关闭
    /// PROCESSING：退款处理中
    /// ABNORMAL：退款异常
    /// </summary>
    [JsonProperty("status")]
    public string? Status { get; set; }

    /// <summary>
    /// 资金账户
    /// </summary>
    [JsonProperty("funds_account")]
    public string? FundsAccount { get; set; }
}
#endregion

#region 返回客户端的参数实体
/// <summary>
/// JSAPI返回参数
/// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter3_1_4.shtml
/// </summary>
public class WeChatPayJsApiParamDto
{
    /// <summary>
    /// 应用ID
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public string? TimeStamp { get; set; }

    /// <summary>
    /// 随机字符串
    /// </summary>
    public string? NonceStr { get; set; }

    /// <summary>
    /// 订单详情扩展字符串
    /// </summary>
    public string? Package { get; set; }

    /// <summary>
    /// 签名方式
    /// </summary>
    public string SignType { get; set; } = "RSA";

    /// <summary>
    /// 签名
    /// </summary>
    public string? PaySign { get; set; }
}

/// <summary>
/// APP返回参数
/// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter3_2_4.shtml
/// </summary>
public class WeChatPayAppParamDto
{
    /// <summary>
    /// 应用ID
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// 商户号
    /// </summary>
    public string? PartnerId { get; set; }

    /// <summary>
    /// 预支付交易会话ID
    /// 示例值： WX1217752501201407033233368018
    /// </summary>
    public string? PrepayId { get; set; }

    /// <summary>
    /// 订单详情扩展字符串
    /// </summary>
    public string Package { get; set; } = "Sign=WXPay";

    /// <summary>
    /// 随机字符串
    /// </summary>
    public string? NonceStr { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public string? TimeStamp { get; set; }

    /// <summary>
    /// 签名
    /// </summary>
    public string? Sign { get; set; }
}

/// <summary>
/// H5返回参数
/// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter3_3_1.shtml
/// </summary>
public class WeChatPayH5ParamDto
{
    /// <summary>
    /// 跳转的URL
    /// </summary>
    public string? Url { get; set; }
}

/// <summary>
/// 扫码返回参数
/// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter3_4_1.shtml
/// </summary>
public class WeChatPayNativeParamDto
{
    /// <summary>
    /// 二维码BASE64图片
    /// </summary>
    public string? CodeData { get; set; }
}
#endregion

/// <summary>
/// 微信公共下单实体
/// </summary>
public class WeChatPayDto
{
    /// <summary>
    /// 商户订单号
    /// </summary>
    [Display(Name = "订单号")]
    [Required(ErrorMessage = "{0}不能为空")]
    public string? OutTradeNo { get; set; }

    /// <summary>
    /// 商品描述
    /// </summary>
    [Display(Name = "商品描述")]
    [Required(ErrorMessage = "{0}不能为空")]
    public string? Description { get; set; }

    /// <summary>
    /// 支付方式(待赋值)
    /// </summary>
    [Display(Name = "支付方式")]
    public int PaymentId { get; set; }

    /// <summary>
    /// 订单总金额(元)
    /// </summary>
    [Display(Name = "总金额(元)")]
    public decimal Total { get; set; } = 0M;

    /// <summary>
    /// 支付成功后跳转链接
    /// </summary>
    [Display(Name = "跳转链接")]
    public string? ReturnUri { get; set; }

    /// <summary>
    /// 授权CODE
    /// </summary>
    [Display(Name = "授权Code")]
    public string? Code { get; set; }
}

/// <summary>
/// 微信授权实体
/// </summary>
public class WeChatPayOAuthDto
{
    /// <summary>
    /// 商户订单号
    /// </summary>
    [Display(Name = "订单号")]
    [Required(ErrorMessage = "{0}不能为空")]
    public string? OutTradeNo { get; set; }

    /// <summary>
    /// 跳转的URI
    /// </summary>
    [Display(Name = "跳转URI")]
    [Required(ErrorMessage = "{0}不能为空")]
    public string? RedirectUri { get; set; }

    /// <summary>
    /// 支付方式ID(待赋值)
    /// </summary>
    [Display(Name = "支付方式")]
    public int PaymentId { get; set; }
}

/// <summary>
/// 微信账户实体
/// </summary>
public class WeChatPayAccountDto
{
    /// <summary>
    /// 站点ID
    /// </summary>
    public int SiteId { get; set; }

    /// <summary>
    /// 微信AppId
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// 微信AppSecret
    /// </summary>
    public string? AppSecret { get; set; }

    /// <summary>
    /// 商户号
    /// </summary>
    public string? MchId { get; set; }

    /// <summary>
    /// CA证书相对路径
    /// </summary>
    public string? CertPath { get; set; }

    /// <summary>
    /// APIV3密钥
    /// </summary>
    public string? Apiv3Key { get; set; }

    /// <summary>
    /// 通知相对地址
    /// </summary>
    public string? NotifyUrl { get; set; }
}

/// <summary>
/// 微信公共退款实体
/// </summary>
public class WeChatPayRefundDto
{
    /// <summary>
    /// 商户订单号
    /// </summary>
    [Display(Name = "订单号")]
    [Required(ErrorMessage = "{0}不能为空")]
    public string? OutTradeNo { get; set; }

    /// <summary>
    /// 退款原因
    /// </summary>
    [Display(Name = "退款原因")]
    public string? Reason { get; set; }

    /// <summary>
    /// 退款ID
    /// </summary>
    public long OutRefundId { get; set; }

    /// <summary>
    /// 支付方式(待赋值)
    /// </summary>
    [Display(Name = "支付方式")]
    public int PaymentId { get; set; }

    /// <summary>
    /// 退款金额(元)
    /// </summary>
    [Display(Name = "退款金额(元)")]
    public decimal Refund { get; set; } = 0M;

    /// <summary>
    /// 原订单金额(元)
    /// </summary>
    [Display(Name = "订单金额(元)")]
    public decimal Total { get; set; } = 0M;
}
