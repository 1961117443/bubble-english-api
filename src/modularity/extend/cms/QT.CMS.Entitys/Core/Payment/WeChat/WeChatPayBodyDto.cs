using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.WeChat;

/// <summary>
/// 基础支付 - JSAPI支付、小程序支付 - 统一下单 - 请求JSON参数
/// </summary>
public class WeChatPayBodyDto
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
    /// 直连商户号
    /// </summary>
    /// <remarks>
    /// 直连商户的商户号，由微信支付生成并下发。
    /// <para>示例值：1230000109</para>
    /// </remarks>
    [JsonProperty("mchid")]
    public string? MchId { get; set; }

    /// <summary>
    /// 商品描述
    /// </summary>
    /// <remarks>
    /// 商品描述
    /// <para>示例值：Image形象店-深圳腾大-QQ公仔</para>
    /// </remarks>
    [JsonProperty("description")]
    public string? Description { get; set; }

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
    /// 交易结束时间
    /// </summary>
    /// <remarks>
    /// 订单失效时间，遵循rfc3339标准格式，格式为YYYY-MM-DDTHH:mm:ss+TIMEZONE，YYYY-MM-DD表示年月日，T出现在字符串中，表示time元素的开头，HH:mm:ss表示时分秒，TIMEZONE表示时区（+08:00表示东八区时间，领先UTC 8小时，即北京时间）。例如：2015-05-20T13:29:35+08:00表示，北京时间2015年5月20日 13点29分35秒。
    /// <para>示例值：2018-06-08T10:34:56+08:00</para>
    /// </remarks>
    [JsonProperty("time_expire")]
    public string? TimeExpire { get; set; }

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
    /// 通知地址
    /// </summary>
    /// <remarks>
    /// 通知URL必须为直接可访问的URL，不允许携带查询串。
    /// 格式：URL
    /// <para>示例值：https://www.weixin.qq.com/wxpay/pay.php</para>
    /// </remarks>
    [JsonProperty("notify_url")]
    public string? NotifyUrl { get; set; }

    /// <summary>
    /// 订单优惠标记
    /// </summary>
    /// <remarks>
    /// 订单优惠标记
    /// <para>示例值：WXG</para>
    /// </remarks>
    [JsonProperty("goods_tag")]
    public string? GoodsTag { get; set; }

    /// <summary>
    /// 订单金额
    /// </summary>
    /// <remarks>
    /// 订单金额信息
    /// </remarks>
    [JsonProperty("amount")]
    public Amount? Amount { get; set; }

    /// <summary>
    /// 支付者
    /// </summary>
    /// <remarks>
    /// 支付者信息
    /// </remarks>
    [JsonProperty("payer")]
    public PayerInfo? Payer { get; set; }

    /// <summary>
    /// 优惠功能
    /// </summary>
    /// <remarks>
    /// 优惠功能
    /// </remarks>
    [JsonProperty("detail")]
    public Detail? Detail { get; set; }

    /// <summary>
    /// 场景信息
    /// </summary>
    /// <remarks>
    /// 支付场景描述
    /// </remarks>
    [JsonProperty("scene_info")]
    public SceneInfo? SceneInfo { get; set; }

    /// <summary>
    /// 结算信息
    /// </summary>
    /// <remarks>
    /// 结算信息
    /// </remarks>
    [JsonProperty("settle_info")]
    public SettleInfo? SettleInfo { get; set; }
}

/// <summary>
/// 支付者信息
/// </summary>
public class PayerInfo
{
    /// <summary>
    /// 用户标识
    /// </summary>
    /// <remarks>
    /// 用户在直连商户appid下的唯一标识。
    /// <para>示例值：oUpF8uMuAJO_M2pxb1Q9zNjWeS6o</para>
    /// </remarks>
    [JsonProperty("openid")]
    public string? OpenId { get; set; }
}

/// <summary>
/// 订单金额
/// </summary>
public class Amount
{
    /// <summary>
    /// 订单金额
    /// </summary>
    /// <remarks>
    /// 订单总金额，单位为分。
    /// <para>示例值：100</para>
    /// </remarks>
    [JsonProperty("total")]
    public int? Total { get; set; }

    /// <summary>
    /// 用户支付金额
    /// </summary>
    /// <remarks>
    /// 用户支付金额，单位为分。
    /// <para>示例值：100</para>
    /// </remarks>
    [JsonProperty("payer_total")]
    public int? PayerTotal { get; set; }

    /// <summary>
    /// 货币类型
    /// </summary>
    /// <remarks>
    /// CNY：人民币，境内商户号仅支持人民币。
    /// <para>示例值：CNY</para>
    /// </remarks>
    [JsonProperty("currency")]
    public string? Currency { get; set; }

    /// <summary>
    /// 用户支付币种
    /// </summary>
    /// <remarks>
    /// 用户支付币种
    /// <para>示例值：CNY</para>
    /// </remarks>
    [JsonProperty("payer_currency")]
    public string? PayerCurrency { get; set; }
}

/// <summary>
/// 单品列表
/// </summary>   
public class GoodsDetail
{
    /// <summary>
    /// 商户侧商品编码
    /// </summary>
    /// <remarks>
    /// 由半角的大小写字母、数字、中划线、下划线中的一种或几种组成。
    /// <para>示例值：商品编码</para>
    /// </remarks>
    [JsonProperty("merchant_goods_id")]
    public string? MerchantGoodsId { get; set; }

    /// <summary>
    /// 微信侧商品编码
    /// </summary>
    /// <remarks>
    /// 微信支付定义的统一商品编号（没有可不传）
    /// <para>示例值：1001</para>
    /// </remarks>
    [JsonProperty("wechatpay_goods_id")]
    public string? WeChatPayGoodsId { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    /// <remarks>
    /// 商品的实际名称
    /// <para>示例值：iPhoneX 256G</para>
    /// </remarks>
    [JsonProperty("goods_name")]
    public string? GoodsName { get; set; }

    /// <summary>
    /// 商品数量
    /// </summary>
    /// <remarks>
    /// 用户购买的数量
    /// <para>示例值：1</para>
    /// </remarks>
    [JsonProperty("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// 商品单价
    /// </summary>
    /// <remarks>
    /// 商品单价，单位为分
    /// <para>示例值：828800</para>
    /// </remarks>
    [JsonProperty("unit_price")]
    public int UnitPrice { get; set; }
}

/// <summary>
/// 优惠功能
/// </summary>
public class Detail
{
    /// <summary>
    /// 订单原价
    /// </summary>
    /// <remarks>
    /// 1、商户侧一张小票订单可能被分多次支付，订单原价用于记录整张小票的交易金额。
    /// 2、当订单原价与支付金额不相等，则不享受优惠。
    /// 3、该字段主要用于防止同一张小票分多次支付，以享受多次优惠的情况，正常支付订单不必上传此参数。
    /// <para>示例值：608800</para>
    /// </remarks>
    [JsonProperty("cost_price")]
    public int? CostPrice { get; set; }

    /// <summary>
    /// 商品小票ID
    /// </summary>
    /// <remarks>
    /// 商家小票ID
    /// <para>示例值：微信123</para>
    /// </remarks>
    [JsonProperty("invoice_id")]
    public string? InvoiceId { get; set; }

    /// <summary>
    /// 单品列表
    /// </summary>
    /// <remarks>
    /// 单品列表信息
    /// 条目个数限制：【1，undefined】
    /// </remarks>
    [JsonProperty("goods_detail")]
    public List<GoodsDetail>? GoodsDetail { get; set; }
}

/// <summary>
/// 场景信息
/// </summary>       
public class SceneInfo
{
    /// <summary>
    /// 用户终端IP
    /// </summary>
    /// <remarks>
    /// 调用微信支付API的机器IP，支持IPv4和IPv6两种格式的IP地址。
    /// <para>示例值：14.23.150.211</para>
    /// </remarks>
    [JsonProperty("payer_client_ip")]
    public string? PayerClientIp { get; set; }

    /// <summary>
    /// 商户端设备号
    /// </summary>
    /// <remarks>
    /// 商户端设备号（门店号或收银设备ID）。
    /// <para>示例值：013467007045764</para>
    /// </remarks>
    [JsonProperty("device_id")]
    public string? DeviceId { get; set; }

    /// <summary>
    /// H5场景信息
    /// </summary>
    [JsonProperty("h5_info")]
    public H5Info? H5Info { get; set; }

    /// <summary>
    /// 商户门店信息
    /// </summary>
    /// <remarks>
    /// 商户门店信息
    /// </remarks>
    [JsonProperty("store_info")]
    public StoreInfo? StoreInfo { get; set; }
}

/// <summary>
/// 结算信息
/// </summary>
public class SettleInfo
{
    /// <summary>
    /// 是否指定分账
    /// </summary>
    /// <remarks>
    /// true：是
    /// false：否
    /// <para>示例值：true</para>
    /// </remarks>
    [JsonProperty("profit_sharing")]
    public bool? ProfitSharing { get; set; }

    /// <summary>
    /// 补差金额
    /// </summary>
    /// <remarks>
    /// SettleInfo.profit_sharing为true时，该金额才生效。
    /// 注意：单笔订单最高补差金额为5000元
    /// <para>示例值：10</para>
    /// </remarks>
    [JsonProperty("subsidy_amount")]
    public long? SubsidyAmount { get; set; }
}

/// <summary>
/// H5场景信息
/// </summary>
public class H5Info
{
    /// <summary>
    /// 场景类型
    /// 示例值：iOS, Android, Wap
    /// </summary>
    [JsonProperty("type")]
    public string? Type { get; set; }

    /// <summary>
    /// 应用名称
    /// 示例值：王者荣耀
    /// </summary>
    [JsonProperty("app_name")]
    public string? AppName { get; set; }

    /// <summary>
    /// 网站URL
    /// 示例值：https://pay.qq.com
    /// </summary>
    [JsonProperty("app_url")]
    public string? AppUrl { get; set; }

    /// <summary>
    /// iOS平台BundleID
    /// 示例值：com.tencent.wzryiO
    /// </summary>
    [JsonProperty("bundle_id")]
    public string? BundleId { get; set; }

    /// <summary>
    /// Android平台PackageName
    /// 示例值：com.tencent.tmgp.sgame
    /// </summary>
    [JsonProperty("package_name")]
    public string? PackageName { get; set; }
}

/// <summary>
/// 商户门店信息
/// </summary>
public class StoreInfo
{
    /// <summary>
    /// 门店编号
    /// </summary>
    /// <remarks>
    /// 商户侧门店编号
    /// <para>示例值：0001</para>
    /// </remarks>
    [JsonProperty("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 门店名称
    /// </summary>
    /// <remarks>
    /// 商户侧门店名称
    /// <para>示例值：腾讯大厦分店</para>
    /// </remarks>
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 地区编码
    /// </summary>
    /// <remarks>
    /// 地区编码，详细请见省市区编号对照表。
    /// <para>示例值：440305</para>
    /// </remarks>
    [JsonProperty("area_code")]
    public string? AreaCode { get; set; }

    /// <summary>
    /// 详细地址
    /// </summary>
    /// <remarks>
    /// 详细的商户门店地址
    /// <para>示例值：广东省深圳市南山区科技中一道10000号</para>
    /// </remarks>
    [JsonProperty("address")]
    public string? Address { get; set; }
}

/// <summary>
/// 通知数据
/// </summary>
public class Resource
{
    /// <summary>
    /// 原始类型
    /// </summary>
    /// <remarks>
    /// <para>示例值：transaction</para>
    /// </remarks>
    [JsonProperty("original_type")]
    public string? OriginalType { get; set; }

    /// <summary>
    /// 加密算法类型
    /// </summary>
    /// <remarks>
    /// 对开启结果数据进行加密的加密算法，目前只支持AEAD_AES_256_GCM
    /// <para>示例值：AEAD_AES_256_GCM</para>
    /// </remarks>
    [JsonProperty("algorithm")]
    public string? Algorithm { get; set; }

    /// <summary>
    /// 数据密文
    /// </summary>
    /// <remarks>
    /// Base64编码后的开启/停用结果数据密文
    /// <para>示例值：sadsadsadsad</para>
    /// </remarks>
    [JsonProperty("ciphertext")]
    public string? Ciphertext { get; set; }

    /// <summary>
    /// 附加数据
    /// </summary>
    /// <remarks>
    /// 附加数据
    /// <para>示例值：fdasfwqewlkja484w</para>
    /// </remarks>
    [JsonProperty("associated_data")]
    public string? AssociatedData { get; set; }

    /// <summary>
    /// 随机串
    /// </summary>
    /// <remarks>
    /// 加密使用的随机串
    /// <para>示例值：fdasflkja484w</para>
    /// </remarks>
    [JsonProperty("nonce")]
    public string? Nonce { get; set; }
}

/// <summary>
/// 优惠功能
/// </summary>
public class PromotionDetail
{
    /// <summary>
    /// 券ID
    /// </summary>
    /// <remarks>
    /// 券ID
    /// <para>示例值：109519</para>
    /// </remarks>
    [JsonProperty("coupon_id")]
    public string? CouponId { get; set; }

    /// <summary>
    /// 优惠名称
    /// </summary>
    /// <remarks>
    /// 优惠名称
    /// <para>示例值：单品惠-6</para>
    /// </remarks>
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 优惠范围
    /// </summary>
    /// <remarks>
    /// GLOBAL：全场代金券
    /// SINGLE：单品优惠
    /// <para>示例值：GLOBAL</para>
    /// </remarks>
    [JsonProperty("scope")]
    public string? Scope { get; set; }

    /// <summary>
    /// 优惠类型
    /// </summary>
    /// <remarks>
    /// CASH：充值
    /// NOCASH：预充值
    /// <para>示例值：CASH</para>
    /// </remarks>
    [JsonProperty("type")]
    public string? Type { get; set; }

    /// <summary>
    /// 优惠券面额
    /// </summary>
    /// <remarks>
    /// 优惠券面额
    /// <para>示例值：100</para>
    /// </remarks>
    [JsonProperty("amount")]
    public int Amount { get; set; }

    /// <summary>
    /// 活动ID
    /// </summary>
    /// <remarks>
    /// 活动ID
    /// <para>示例值：931386</para>
    /// </remarks>
    [JsonProperty("stock_id")]
    public string? StockId { get; set; }

    /// <summary>
    /// 微信出资
    /// </summary>
    /// <remarks>
    /// 微信出资，单位为分
    /// <para>示例值：0</para>
    /// </remarks>
    [JsonProperty("wechatpay_contribute")]
    public int? WeChatPayContribute { get; set; }

    /// <summary>
    /// 商户出资
    /// </summary>
    /// <remarks>
    /// 商户出资，单位为分
    /// <para>示例值：0</para>
    /// </remarks>
    [JsonProperty("merchant_contribute")]
    public long? MerchantContribute { get; set; }

    /// <summary>
    /// 其他出资
    /// </summary>
    /// <remarks>
    /// 其他出资，单位为分
    /// <para>示例值：0</para>
    /// </remarks>
    [JsonProperty("other_contribute")]
    public long? OtherContribute { get; set; }

    /// <summary>
    /// 优惠币种
    /// </summary>
    /// <remarks>
    /// CNY：人民币，境内商户号仅支持人民币。
    /// <para>示例值：CNY</para>
    /// </remarks>
    [JsonProperty("currency")]
    public string? Currency { get; set; }

    /// <summary>
    /// 单品列表
    /// 单品列表信息
    /// </summary>
    /// <remarks>
    /// 单品列表信息
    /// </remarks>
    [JsonProperty("goods_detail")]
    public List<PromotionGoodsDetail>? GoodsDetail { get; set; }
}

/// <summary>
/// 单品列表信息
/// </summary>
public class PromotionGoodsDetail
{
    /// <summary>
    /// 商品编码
    /// </summary>
    /// <remarks>
    /// 商品编码
    /// <para>示例值：M1006</para>
    /// </remarks>
    [JsonProperty("goods_id")]
    public string? GoodsId { get; set; }

    /// <summary>
    /// 商品数量
    /// </summary>
    /// <remarks>
    /// 用户购买的数量
    /// <para>示例值：1</para>
    /// </remarks>
    [JsonProperty("quantity")]
    public int? Quantity { get; set; }

    /// <summary>
    /// 商品单价
    /// </summary>
    /// <remarks>
    /// 商品单价，单位为分
    /// <para>示例值：100</para>
    /// </remarks>
    [JsonProperty("unit_price")]
    public long? UnitPrice { get; set; }

    /// <summary>
    /// 商品优惠金额
    /// </summary>
    /// <remarks>
    /// 商品优惠金额
    /// <para>示例值：0  </para>
    /// </remarks>
    [JsonProperty("discount_amount")]
    public long? DiscountAmount { get; set; }

    /// <summary>
    /// 商品备注
    /// </summary>
    /// <remarks>
    /// 商品备注信息
    /// <para>示例值：商品备注信息</para>
    /// </remarks>
    [JsonProperty("goods_remark")]
    public string? GoodsRemark { get; set; }
}

/// <summary>
/// 申请退款 - 请求JSON参数
/// </summary>
public class WeChatPayRefundBodyDto
{
    /// <summary>
    /// 商户订单号
    /// </summary>
    /// <remarks>
    /// 商户系统内部订单号，只能是数字、大小写字母_-*且在同一个商户号下唯一，详见【商户订单号】。
    /// 特殊规则：最小字符长度为6
    /// <para>示例值：1217752501201407033233368018</para>
    /// </remarks>
    [JsonProperty("out_trade_no")]
    [MinLength(6, ErrorMessage = "{0}至少{1}位字符")]
    [MaxLength(32, ErrorMessage = "{0}最多{2}位字符")]
    public string? OutTradeNo { get; set; }

    /// <summary>
    /// 商户退款单号
    /// </summary>
    [JsonProperty("out_refund_no")]
    [MinLength(1, ErrorMessage = "{0}至少{1}位字符")]
    [MaxLength(64, ErrorMessage = "{0}最多{2}位字符")]
    public string? OutRefundNo { get; set; }

    /// <summary>
    /// 退款原因
    /// </summary>
    /// <remarks>
    /// 若商户传入，会在下发给用户的退款消息中体现退款原因
    /// </remarks>
    [JsonProperty("reason")]
    public string? Reason { get; set; }

    /// <summary>
    /// 退款结果回调url
    /// </summary>
    /// <remarks>
    /// 通知url必须为外网可访问的url，不能携带参数。
    /// 格式：URL
    /// <para>示例值：https://weixin.qq.com</para>
    /// </remarks>
    [JsonProperty("notify_url")]
    public string? NotifyUrl { get; set; }

    /// <summary>
    /// 退款资金来源
    /// </summary>
    [JsonProperty("funds_account")]
    public string? FundsAccount { get; set; }

    /// <summary>
    /// 订单金额
    /// </summary>
    /// <remarks>
    /// 订单金额信息
    /// </remarks>
    [JsonProperty("amount")]
    public RefundBodyAmount? Amount { get; set; }
}

/// <summary>
/// 退款订单金额信息
/// </summary>
public class RefundBodyAmount
{
    /// <summary>
    /// 退款金额
    /// </summary>
    /// <remarks>
    /// 单位为分，只能为整数，不能超过原订单支付金额。
    /// <para>示例值：100</para>
    /// </remarks>
    [JsonProperty("refund")]
    public int? Refund { get; set; }

    /// <summary>
    /// 原订单金额
    /// </summary>
    /// <remarks>
    /// 原支付交易的订单总金额，单位为分，只能为整数。
    /// <para>示例值：100</para>
    /// </remarks>
    [JsonProperty("total")]
    public int? Total { get; set; }

    /// <summary>
    /// 退款币种
    /// </summary>
    /// <remarks>
    /// 符合ISO 4217标准的三位字母代码，目前只支持人民币：CNY。
    /// <para>示例值：CNY</para>
    /// </remarks>
    [JsonProperty("currency")]
    public string? Currency { get; set; } = "CNY";
}
