using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys;

/// <summary>
/// 统一下单实体
/// </summary>
public class AlipayTradeDto
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
    /// 支付成功后跳转链接
    /// </summary>
    [Display(Name = "跳转链接")]
    public string? ReturnUri { get; set; }

    /// <summary>
    /// 支付方式(待赋值)
    /// </summary>
    [Display(Name = "支付方式")]
    public int PaymentId { get; set; }

    /// <summary>
    /// 订单总金额(元)(待赋值)
    /// </summary>
    [Display(Name = "总金额(元)")]
    public decimal Total { get; set; } = 0M;
}

/// <summary>
/// 统一退款实体
/// </summary>
public class AlipayRefundDto
{
    /// <summary>
    /// 商户订单号
    /// </summary>
    [Display(Name = "订单号")]
    [Required(ErrorMessage = "{0}不能为空")]
    public string? OutTradeNo { get; set; }

    /// <summary>
    /// 退款ID
    /// </summary>
    [Required(ErrorMessage = "{0}不能为空")]
    public long OutRefundId { get; set; }

    /// <summary>
    /// 退款金额(元)
    /// </summary>
    [Display(Name = "退款金额(元)")]
    public decimal Refund { get; set; } = 0M;

    /// <summary>
    /// 退款原因
    /// </summary>
    [Display(Name = "退款原因")]
    public string? Reason { get; set; }

    /// <summary>
    /// 支付方式(待赋值)
    /// </summary>
    [Display(Name = "支付方式")]
    public int PaymentId { get; set; }
}

/// <summary>
/// 支付宝账户实体
/// </summary>
public class AlipayAccountDto
{
    /// <summary>
    /// 站点ID
    /// </summary>
    public int SiteId { get; set; }

    /// <summary>
    /// 应用Id
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// 普通公钥：支付宝公钥 RSA公钥
    /// 为支付宝开放平台-支付宝公钥
    /// </summary>
    public string? AlipayPublicKey { get; set; }

    /// <summary>
    /// 普通公钥：应用私钥 RSA私钥
    /// 为“支付宝开放平台开发助手”所生成的应用私钥
    /// </summary>
    public string? AppPrivateKey { get; set; }

    /// <summary>
    /// 支付宝服务器主动通知商户服务器里指定的API接口
    /// </summary>
    public string? NotifyUrl { get; set; }

    /// <summary>
    /// 支付方式接口类型
    /// </summary>
    public string? NotifyType { get; set; }
}

#region 返回客户端的参数实体
/// <summary>
/// 电脑、手机支付返回实体
/// </summary>
public class AlipayPageParamDto
{
    /// <summary>
    /// 跳转的URL
    /// </summary>
    public string? Url { get; set; }
}

/// <summary>
/// 退款响应返回实体
/// </summary>
public class AlipayRefundParamDto
{
    /// <summary>
    /// 响应业务参数
    /// </summary>
    [JsonProperty("alipay_trade_refund_response")]
    public AlipayTradeRefundResponse? Response { get; set; }

    /// <summary>
    /// 签名字符串
    /// </summary>
    [JsonProperty("sign")]
    public string? Sign { get; set; }
}

/// <summary>
/// 退款响应业务参数
/// </summary>
public class AlipayTradeRefundResponse
{
    /// <summary>
    /// 网关返回码
    /// </summary>
    [JsonProperty("code")]
    public string? Code { get; set; }

    /// <summary>
    /// 网关返回码
    /// </summary>
    [JsonProperty("msg")]
    public string? Msg { get; set; }

    /// <summary>
    /// 支付宝交易号
    /// </summary>
    [JsonProperty("trade_no")]
    public string? TradeNo { get; set; }

    /// <summary>
    /// 商户订单号
    /// </summary>
    [JsonProperty("out_trade_no")]
    public string? OutTradeNo { get; set; }

    /// <summary>
    /// 用户的登录id
    /// </summary>
    [JsonProperty("buyer_logon_id")]
    public string? BuyerLogonId { get; set; }

    /// <summary>
    /// 退款状态
    /// 成功则为Y
    /// </summary>
    [JsonProperty("fund_change")]
    public string? FundChange { get; set; }

    /// <summary>
    /// 退款金额(元)
    /// </summary>
    [JsonProperty("refund_fee")]
    public decimal RefundFeet { get; set; } = 0M;

    /// <summary>
    /// 门店名称
    /// </summary>
    [JsonProperty("store_name")]
    public string? StoreName { get; set; }

    /// <summary>
    /// 支付宝的用户id
    /// </summary>
    [JsonProperty("buyer_user_id")]
    public string? BuyerUserId { get; set; }

    /// <summary>
    /// 本次商户实际退回金额(元)
    /// </summary>
    [JsonProperty("send_back_fee")]
    public string? SendBackFee { get; set; }
}
#endregion
