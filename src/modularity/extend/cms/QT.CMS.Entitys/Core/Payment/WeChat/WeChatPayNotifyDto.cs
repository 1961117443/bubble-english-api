using Newtonsoft.Json;
using QT.CMS.Entitys.WeChat;

namespace QT.CMS.Entitys;

/// <summary>
/// WeChatPay V3通知报文
/// </summary>
public class WeChatPayNotifyDto
{
    /// <summary>
    /// 通知ID
    /// </summary>
    /// <remarks>
    /// 通知的唯一ID
    /// <para>示例值：EV-2018022511223320873</para>
    /// </remarks>
    [JsonProperty("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 通知创建时间
    /// </summary>
    /// <remarks>
    /// 通知创建的时间，格式为yyyyMMddHHmmss
    /// <para>示例值：20180225112233</para>
    /// </remarks>
    [JsonProperty("create_time")]
    public string? CreateTime { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    /// <remarks>
    /// 通知的类型，支付成功通知的类型为TRANSACTION.SUCCESS
    /// <para>示例值：TRANSACTION.SUCCESS</para>
    /// </remarks>
    [JsonProperty("event_type")]
    public string? EventType { get; set; }

    /// <summary>
    /// 通知数据类型
    /// </summary>
    /// <remarks>
    /// 通知的资源数据类型，支付成功通知为encrypt-resource
    /// <para>示例值：encrypt-resource</para>
    /// </remarks>
    [JsonProperty("resource_type")]
    public string? ResourceType { get; set; }

    /// <summary>
    /// 通知数据
    /// </summary>
    /// <remarks>
    /// 通知资源数据
    /// json格式，见示例
    /// </remarks>
    [JsonProperty("resource")]
    public Resource? Resource { get; set; }

    /// <summary>
    /// 概要
    /// </summary>
    /// <remarks>
    /// <para>示例值：支付成功</para>
    /// </remarks>
    [JsonProperty("summary")]
    public string? Summary { get; set; }
}

/// <summary>
/// 支付结果解密实体
/// </summary>
public class WeChatPayNotifyDecryptDto
{
    /// <summary>
    /// 应用ID
    /// </summary>
    /// <remarks>
    /// 直连商户申请的公众号或移动应用appid。
    /// <para>示例值：wxd678efh567hg6787</para>
    /// </remarks>
    [JsonProperty("appid")]
    public string? AppId { get; set; }

    /// <summary>
    /// 商户号
    /// </summary>
    /// <remarks>
    /// 直连商户的商户号，由微信支付生成并下发。
    /// <para>示例值：1230000109</para>
    /// </remarks>
    [JsonProperty("mchid")]
    public string? MchId { get; set; }

    /// <summary>
    /// 商户订单号
    /// </summary>
    /// <remarks>
    /// 商户系统内部订单号，只能是数字、大小写字母_-*且在同一个商户号下唯一，详见【商户订单号】。
    /// 特殊规则：最小字符长度为6
    /// <para>示例值：1217752501201407033233368018</para>
    /// </remarks>
    [JsonProperty("out_trade_no")]
    public string? OutTradeNo { get; set; }

    /// <summary>
    /// 微信支付订单号
    /// </summary>
    /// <remarks>
    /// 微信支付系统生成的订单号。
    /// <para>示例值：1217752501201407033233368018</para>
    /// </remarks>
    [JsonProperty("transaction_id")]
    public string? TransactionId { get; set; }

    /// <summary>
    /// 交易类型
    /// </summary>
    /// <remarks>
    /// 交易类型，枚举值：
    /// JSAPI：公众号支付
    /// NATIVE：扫码支付
    /// APP：APP支付
    /// MICROPAY：付款码支付
    /// MWEB：H5支付
    /// FACEPAY：刷脸支付
    /// <para>示例值：MICROPAY</para>
    /// </remarks>
    [JsonProperty("trade_type")]
    public string? TradeType { get; set; }

    /// <summary>
    /// 交易状态
    /// </summary>
    /// <remarks>
    /// 交易状态，枚举值：
    /// SUCCESS：支付成功
    /// REFUND：转入退款
    /// NOTPAY：未支付
    /// CLOSED：已关闭
    /// REVOKED：已撤销（付款码支付）
    /// USERPAYING：用户支付中（付款码支付）
    /// PAYERROR：支付失败(其他原因，如银行返回失败)
    /// <para>示例值：SUCCESS</para>
    /// </remarks>
    [JsonProperty("trade_state")]
    public string? TradeState { get; set; }

    /// <summary>
    /// 交易状态描述
    /// </summary>
    /// <remarks>
    /// 交易状态描述
    /// <para>示例值：支付失败，请重新下单支付</para>
    /// </remarks>
    [JsonProperty("trade_state_desc")]
    public string? TradeStateDesc { get; set; }

    /// <summary>
    /// 付款银行
    /// </summary>
    /// <remarks>
    /// 银行类型，采用字符串类型的银行标识。
    /// <para>示例值：CMC</para>
    /// </remarks>
    [JsonProperty("bank_type")]
    public string? BankType { get; set; }

    /// <summary>
    /// 附加数据
    /// </summary>
    /// <remarks>
    /// 附加数据，在查询API和支付通知中原样返回，可作为自定义参数使用
    /// <para>示例值：自定义数据</para>
    /// </remarks>
    [JsonProperty("attach")]
    public string? Attach { get; set; }

    /// <summary>
    /// 支付完成时间
    /// </summary>
    /// <remarks>
    /// 支付完成时间，遵循rfc3339标准格式，格式为YYYY-MM-DDTHH:mm:ss+TIMEZONE，YYYY-MM-DD表示年月日，T出现在字符串中，表示time元素的开头，HH:mm:ss表示时分秒，TIMEZONE表示时区（+08:00表示东八区时间，领先UTC 8小时，即北京时间）。例如：2015-05-20T13:29:35+08:00表示，北京时间2015年5月20日 13点29分35秒。
    /// <para>示例值：2018-06-08T10:34:56+08:00</para>
    /// </remarks>
    [JsonProperty("success_time")]
    public string? SuccessTime { get; set; }

    /// <summary>
    /// 支付者
    /// </summary>
    /// <remarks>
    /// <para>示例值：见请求示例</para>
    /// </remarks>
    [JsonProperty("payer")]
    public PayerInfo? Payer { get; set; }

    /// <summary>
    /// 订单金额
    /// </summary>
    /// <remarks>
    /// 订单金额信息
    /// </remarks>
    [JsonProperty("amount")]
    public Amount? Amount { get; set; }

    /// <summary>
    /// 场景信息
    /// </summary>
    /// <remarks>
    /// 支付场景描述
    /// </remarks>
    [JsonProperty("scene_info")]
    public SceneInfo? SceneInfo { get; set; }

    /// <summary>
    /// 优惠功能
    /// </summary>
    /// <remarks>
    /// 优惠功能，享受优惠时返回该字段。
    /// </remarks>
    [JsonProperty("promotion_detail")]
    public List<PromotionDetail>? PromotionDetail { get; set; }
}

/// <summary>
/// 支付结果返回实体
/// </summary>
public class WeChatPayNotifyResultDto
{
    /// <summary>
    /// 返回状态码,错误码，SUCCESS为清算机构接收成功，其他错误码为失败。
    /// </summary>
    [JsonProperty("code")]
    public string Code { set; get; } = "SUCCESS";

    /// <summary>
    /// 返回信息，如非空，为错误原因。
    /// </summary>
    [JsonProperty("message")]
    public string Message { set; get; } = "SUCCESS";
}
