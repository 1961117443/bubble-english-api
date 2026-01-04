using Newtonsoft.Json;

namespace QT.CMS.Entitys.Alipay;

/// <summary>
/// APP支付参数
/// </summary>
public class AlipayTradeAppDto
{
    /// <summary>
    /// 签约参数。如果希望在sdk中支付并签约，需要在这里传入签约信息。 周期扣款场景 product_code 为 CYCLE_PAY_AUTH 时必填。
    /// </summary>
    [JsonProperty("agreement_sign_params")]
    public SignParams? AgreementSignParams { get; set; }

    /// <summary>
    /// 对一笔交易的具体描述信息。如果是多种商品，请将商品描述字符串累加传给body。
    /// </summary>
    [JsonProperty("body")]
    public string? Body { get; set; }

    /// <summary>
    /// 商户传入业务信息，具体值要和支付宝约定，应用于安全，营销等参数直传场景，格式为json格式
    /// </summary>
    [JsonProperty("business_params")]
    public string? BusinessParams { get; set; }

    /// <summary>
    /// 禁用渠道，用户不可用指定渠道支付  当有多个渠道时用“,”分隔  注，与enable_pay_channels互斥
    /// </summary>
    [JsonProperty("disable_pay_channels")]
    public string? DisablePayChannels { get; set; }

    /// <summary>
    /// 可用渠道，优先推荐用户使用的支付渠道。 注：当有多个渠道时用“,”分隔注，与disable_pay_channels互斥。
    /// </summary>
    [JsonProperty("enable_pay_channels")]
    public string? EnablePayChannels { get; set; }

    /// <summary>
    /// 外部指定买家
    /// </summary>
    [JsonProperty("ext_user_info")]
    public ExtUserInfo? ExtUserInfo { get; set; }

    /// <summary>
    /// 业务扩展参数
    /// </summary>
    [JsonProperty("extend_params")]
    public ExtendParams? ExtendParams { get; set; }

    /// <summary>
    /// 订单包含的商品列表信息，json格式，其它说明详见商品明细说明
    /// </summary>
    [JsonProperty("goods_detail")]
    public List<GoodsDetail>? GoodsDetail { get; set; }

    /// <summary>
    /// 商品主类型，取值如下： 0：虚拟类商品； 1：实物类商品。
    /// </summary>
    [JsonProperty("goods_type")]
    public string? GoodsType { get; set; }

    /// <summary>
    /// 开票信息
    /// </summary>
    [JsonProperty("invoice_info")]
    public InvoiceInfo? InvoiceInfo { get; set; }

    /// <summary>
    /// 商户原始订单号，最大长度限制32位
    /// </summary>
    [JsonProperty("merchant_order_no")]
    public string? MerchantOrderNo { get; set; }

    /// <summary>
    /// 商户订单号，由商家自定义，需保证商家系统中唯一。仅支持数字、字母、下划线。
    /// </summary>
    [JsonProperty("out_trade_no")]
    public string? OutTradeNo { get; set; }

    /// <summary>
    /// 公用回传参数，如果请求时传递了该参数，则返回给商户时会回传该参数。支付宝只会在同步返回（包括跳转回商户网站）和异步通知时将该参数原样返回。本参数必须进行UrlEncode之后才可以发送给支付宝。
    /// </summary>
    [JsonProperty("passback_params")]
    public string? PassbackParams { get; set; }

    /// <summary>
    /// 销售产品码，商家和支付宝签约的产品码，默认为 QUICK_MSECURITY_PAY（App支付）。枚举支持： QUICK_MSECURITY_PAY：App支付； CYCLE_PAY_AUTH：周期扣款。 周期扣款产品场景必填。
    /// </summary>
    [JsonProperty("product_code")]
    public string? ProductCode { get; set; }

    /// <summary>
    /// 优惠参数  注：仅与支付宝协商后可用
    /// </summary>
    [JsonProperty("promo_params")]
    public string? PromoParams { get; set; }

    /// <summary>
    /// 描述分账信息，json格式，详见分账参数说明
    /// </summary>
    [JsonProperty("royalty_info")]
    public RoyaltyInfo? RoyaltyInfo { get; set; }

    /// <summary>
    /// 收款支付宝用户ID。 如果该值为空，则默认为商户签约账号对应的支付宝用户ID
    /// </summary>
    [JsonProperty("seller_id")]
    public string? SellerId { get; set; }

    /// <summary>
    /// 描述结算信息，json格式，详见结算参数说明
    /// </summary>
    [JsonProperty("settle_info")]
    public SettleInfo? SettleInfo { get; set; }

    /// <summary>
    /// 指定渠道，目前仅支持传入pcredit  若由于用户原因渠道不可用，用户可选择是否用其他渠道支付。  注：该参数不可与花呗分期参数同时传入
    /// </summary>
    [JsonProperty("specified_channel")]
    public string? SpecifiedChannel { get; set; }

    /// <summary>
    /// 商户门店编号
    /// </summary>
    [JsonProperty("store_id")]
    public string? StoreId { get; set; }

    /// <summary>
    /// 间连受理商户信息体，当前只对特殊银行机构特定场景下使用此字段
    /// </summary>
    [JsonProperty("sub_merchant")]
    public SubMerchant? SubMerchant { get; set; }

    /// <summary>
    /// 商品标题/交易标题/订单标题/订单关键字等。  注意：不可使用特殊字符，如 /，=，& 等。
    /// </summary>
    [JsonProperty("subject")]
    public string? Subject { get; set; }

    /// <summary>
    /// 绝对超时时间，格式为yyyy-MM-dd HH:mm。
    /// </summary>
    [JsonProperty("time_expire")]
    public string? TimeExpire { get; set; }

    /// <summary>
    /// 该笔订单允许的最晚付款时间，逾期将关闭交易。取值范围：5m～15d。m-分钟，h-小时，d-天，1c-当天（1c-当天的情况下，无论交易何时创建，都在0点关闭）。 该参数数值不接受小数点， 如 1.5h，可转换为 90m。
    /// </summary>
    [JsonProperty("timeout_express")]
    public string? TimeoutExpress { get; set; }

    /// <summary>
    /// 订单总金额，单位为人民币（元），取值范围为 0.01~100000000.00，精确到小数点后两位。
    /// </summary>
    [JsonProperty("total_amount")]
    public string? TotalAmount { get; set; }
}

/// <summary>
/// 电脑支付参数
/// </summary>
public class AlipayTradePageDto
{
    /// <summary>
    /// 商户订单号。64 个字符以内的大小，仅支持字母、数字、下划线。需保证该参数在商户端不重复。
    /// </summary>
    [JsonProperty("out_trade_no")]
    public string? OutTradeNo { get; set; }

    /// <summary>
    /// 销售产品码，与支付宝签约的产品码名称。  注：目前仅支持FAST_INSTANT_TRADE_PAY
    /// </summary>
    [JsonProperty("product_code")]
    public string? ProductCode { get; set; }

    /// <summary>
    /// 订单总金额，单位为人民币（元），取值范围为 0.01~100000000.00，精确到小数点后两位。
    /// </summary>
    [JsonProperty("total_amount")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 商品标题/交易标题/订单标题/订单关键字等。  注意：不可使用特殊字符，如 /，=，& 等。
    /// </summary>
    [JsonProperty("subject")]
    public string? Subject { get; set; }

    /// <summary>
    /// 订单描述
    /// </summary>
    [JsonProperty("body")]
    public string? Body { get; set; }

    /// <summary>
    /// 绝对超时时间，格式为yyyy-MM-dd HH:mm:ss
    /// </summary>
    [JsonProperty("time_expire")]
    public string? TimeExpire { get; set; }

    /// <summary>
    /// 该笔订单允许的最晚付款时间，逾期将关闭交易。取值范围：1m～15d。m-分钟，h-小时，d-天，1c-当天（1c-当天的情况下，无论交易何时创建，都在0点关闭）。 该参数数值不接受小数点， 如 1.5h，可转换为 90m
    /// </summary>
    [JsonProperty("timeout_express")]
    public string? TimeoutExpress { get; set; }

    /// <summary>
    /// 订单包含的商品列表信息，json格式。
    /// </summary>
    [JsonProperty("goods_detail")]
    public List<GoodsDetail>? GoodsDetail { get; set; }

    /// <summary>
    /// 公用回传参数，如果请求时传递了该参数，则返回给商户时会回传该参数。支付宝会在异步通知时将该参数原样返回。本参数必须进行UrlEncode之后才可以发送给支付宝。
    /// </summary>
    [JsonProperty("passback_params")]
    public string? PassbackParams { get; set; }

    /// <summary>
    /// 业务扩展参数
    /// </summary>
    [JsonProperty("extend_params")]
    public ExtendParams? ExtendParams { get; set; }

    /// <summary>
    /// 商品主类型 ，枚举支持： 0：虚拟类商品； 1：实物类商品。 注：虚拟类商品不支持使用花呗渠道
    /// </summary>
    [JsonProperty("goods_type")]
    public string? GoodsType { get; set; }

    /// <summary>
    /// 优惠参数  注：仅与支付宝协商后可用
    /// </summary>
    [JsonProperty("promo_params")]
    public string? PromoParams { get; set; }

    /// <summary>
    /// 描述分账信息，json格式。
    /// </summary>
    [JsonProperty("royalty_info")]
    public RoyaltyInfo? RoyaltyInfo { get; set; }

    /// <summary>
    /// 间连受理商户信息体，当前只对特殊银行机构特定场景下使用此字段
    /// </summary>
    [JsonProperty("sub_merchant")]
    public SubMerchant? SubMerchant { get; set; }

    /// <summary>
    /// 商户原始订单号，最大长度限制32位
    /// </summary>
    [JsonProperty("merchant_order_no")]
    public string? MerchantOrderNo { get; set; }

    /// <summary>
    /// 可用渠道,用户只能在指定渠道范围内支付，多个渠道以逗号分割  注，与disable_pay_channels互斥  渠道列表：https://docs.open.alipay.com/common/wifww7
    /// </summary>
    [JsonProperty("enable_pay_channels")]
    public string? EnablePayChannels { get; set; }

    /// <summary>
    /// 商户门店编号。指商户创建门店时输入的门店编号。
    /// </summary>
    [JsonProperty("store_id")]
    public string? StoreId { get; set; }

    /// <summary>
    /// 禁用渠道,用户不可用指定渠道支付，多个渠道以逗号分割
    /// 注，与enable_pay_channels互斥
    /// </summary>
    [JsonProperty("disable_pay_channels")]
    public string? DisablePayChannels { get; set; }

    /// <summary>
    /// PC扫码支付的方式，支持前置模式和    跳转模式。  前置模式是将二维码前置到商户  的订单确认页的模式。需要商户在  自己的页面中以 iframe 方式请求  支付宝页面。具体分为以下几种：  0：订单码-简约前置模式，对应 iframe 宽度不能小于600px，高度不能小于300px；  1：订单码-前置模式，对应iframe 宽度不能小于 300px，高度不能小于600px；  3：订单码-迷你前置模式，对应 iframe 宽度不能小于 75px，高度不能小于75px；  4：订单码-可定义宽度的嵌入式二维码，商户可根据需要设定二维码的大小。    跳转模式下，用户的扫码界面是由支付宝生成的，不在商户的域名下。  2：订单码-跳转模式
    /// </summary>
    [JsonProperty("qr_pay_mode")]
    public string? QrPayMode { get; set; }

    /// <summary>
    /// 商户自定义二维码宽度  注：qr_pay_mode=4时该参数生效
    /// </summary>
    [JsonProperty("qrcode_width")]
    public long QrcodeWidth { get; set; }

    /// <summary>
    /// 描述结算信息，json格式。
    /// </summary>
    [JsonProperty("settle_info")]
    public SettleInfo? SettleInfo { get; set; }

    /// <summary>
    /// 开票信息
    /// </summary>
    [JsonProperty("invoice_info")]
    public InvoiceInfo? InvoiceInfo { get; set; }

    /// <summary>
    /// 签约参数，支付后签约场景使用
    /// </summary>
    [JsonProperty("agreement_sign_params")]
    public AgreementSignParams? AgreementSignParams { get; set; }

    /// <summary>
    /// 请求后页面的集成方式。  取值范围：  1. ALIAPP：支付宝钱包内  2. PCWEB：PC端访问  默认值为PCWEB。
    /// </summary>
    [JsonProperty("integration_type")]
    public string? IntegrationType { get; set; }

    /// <summary>
    /// 请求来源地址。如果使用ALIAPP的集成方式，用户中途取消支付会返回该地址。
    /// </summary>
    [JsonProperty("request_from_url")]
    public string? RequestFromUrl { get; set; }

    /// <summary>
    /// 商户传入业务信息，具体值要和支付宝约定，应用于安全，营销等参数直传场景，格式为json格式
    /// </summary>
    [JsonProperty("business_params")]
    public string? BusinessParams { get; set; }

    /// <summary>
    /// 外部指定买家
    /// </summary>
    [JsonProperty("ext_user_info")]
    public ExtUserInfo? ExtUserInfo { get; set; }
}

/// <summary>
/// 手机支付参数
/// </summary>
public class AlipayTradeWapDto
{
    /// <summary>
    /// 对一笔交易的具体描述信息。如果是多种商品，请将商品描述字符串累加传给body。
    /// </summary>
    [JsonProperty("body")]
    public string? Body { get; set; }

    /// <summary>
    /// 商品标题/交易标题/订单标题/订单关键字等。  注意：不可使用特殊字符，如 /，=，& 等。
    /// </summary>
    [JsonProperty("subject")]
    public string? Subject { get; set; }

    /// <summary>
    /// 商户网站订单号，由商家自定义，需保证商家系统中唯一。仅支持数字、字母、下划线。
    /// </summary>
    [JsonProperty("out_trade_no")]
    public string? OutTradeNo { get; set; }

    /// <summary>
    /// 该笔订单允许的最晚付款时间，逾期将关闭交易。取值范围：5m～15d。m-分钟，h-小时，d-天，1c-当天（1c-当天的情况下，无论交易何时创建，都在0点关闭）。 该参数数值不接受小数点， 如 1.5h，可转换为 90m。
    /// </summary>
    [JsonProperty("timeout_express")]
    public string? TimeoutExpress { get; set; }

    /// <summary>
    /// 绝对超时时间，格式为yyyy-MM-dd HH:mm。
    /// </summary>
    [JsonProperty("time_expire")]
    public string? TimeExpire { get; set; }

    /// <summary>
    /// 订单总金额，单位为人民币（元），取值范围为 0.01~100000000.00，精确到小数点后两位。
    /// </summary>
    [JsonProperty("total_amount")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 针对用户授权接口，获取用户相关数据时，用于标识用户授权关系
    /// </summary>
    [JsonProperty("auth_token")]
    public string? AuthToken { get; set; }

    /// <summary>
    /// 商品主类型，取值如下： 0：虚拟类商品； 1：实物类商品。
    /// </summary>
    [JsonProperty("goods_type")]
    public string? GoodsType { get; set; }

    /// <summary>
    /// 用户付款中途退出返回商户网站的地址
    /// </summary>
    [JsonProperty("quit_url")]
    public string? QuitUrl { get; set; }

    /// <summary>
    /// 公用回传参数，如果请求时传递了该参数，则返回给商户时会回传该参数。支付宝只会在同步返回（包括跳转回商户网站）和异步通知时将该参数原样返回。本参数必须进行UrlEncode之后才可以发送给支付宝。
    /// </summary>
    [JsonProperty("passback_params")]
    public string? PassbackParams { get; set; }

    /// <summary>
    /// 销售产品码，商家和支付宝签约的产品码
    /// </summary>
    [JsonProperty("product_code")]
    public string? ProductCode { get; set; }

    /// <summary>
    /// 优惠参数  注：仅与支付宝协商后可用
    /// </summary>
    [JsonProperty("promo_params")]
    public string? PromoParams { get; set; }

    /// <summary>
    /// 业务扩展参数
    /// </summary>
    [JsonProperty("extend_params")]
    public ExtendParams? ExtendParams { get; set; }

    /// <summary>
    /// 商户原始订单号，最大长度限制32位
    /// </summary>
    [JsonProperty("merchant_order_no")]
    public string? MerchantOrderNo { get; set; }

    /// <summary>
    /// 可用渠道，优先推荐用户使用的支付渠道。 注：当有多个渠道时用“,”分隔注，与disable_pay_channels互斥。
    /// </summary>
    [JsonProperty("enable_pay_channels")]
    public string? EnablePayChannels { get; set; }

    /// <summary>
    /// 禁用渠道，用户不可用指定渠道支付  当有多个渠道时用“,”分隔  注，与enable_pay_channels互斥
    /// </summary>
    [JsonProperty("disable_pay_channels")]
    public string? DisablePayChannels { get; set; }

    /// <summary>
    /// 收款支付宝用户ID。 如果该值为空，则默认为商户签约账号对应的支付宝用户ID
    /// </summary>
    [JsonProperty("seller_id")]
    public string? SellerId { get; set; }

    /// <summary>
    /// 订单包含的商品列表信息，json格式，其它说明详见商品明细说明
    /// </summary>
    [JsonProperty("goods_detail")]
    public List<GoodsDetail>? GoodsDetail { get; set; }

    /// <summary>
    /// 指定渠道，目前仅支持传入pcredit  若由于用户原因渠道不可用，用户可选择是否用其他渠道支付。  注：该参数不可与花呗分期参数同时传入
    /// </summary>
    [JsonProperty("specified_channel")]
    public string? SpecifiedChannel { get; set; }

    /// <summary>
    /// 商户传入业务信息，具体值要和支付宝约定，应用于安全，营销等参数直传场景，格式为json格式
    /// </summary>
    [JsonProperty("business_params")]
    public string? BusinessParams { get; set; }

    /// <summary>
    /// 外部指定买家
    /// </summary>
    [JsonProperty("ext_user_info")]
    public ExtUserInfo? ExtUserInfo { get; set; }
}

/// <summary>
/// 申请退款参数
/// </summary>
public class AlipayTradeRefundDto
{
    /// <summary>
    /// 商户订单号
    /// </summary>
    [JsonProperty("out_trade_no")]
    public string? OutTradeNo { get; set; }

    /// <summary>
    /// 退款金额(元)
    /// </summary>
    [JsonProperty("refund_amount")]
    public decimal RefundAmount { get; set; } = 0M;

    /// <summary>
    /// 退款原因
    /// </summary>
    [JsonProperty("refund_reason")]
    public string? RefundReason { get; set; }
}


#region 成员参数
/// <summary>
/// AccessParams Data Structure.
/// </summary>
public class AccessParams
{
    /// <summary>
    /// 目前支持以下值：  1. ALIPAYAPP  （钱包h5页面签约）  2. QRCODE(扫码签约)  3. QRCODEORSMS(扫码签约或者短信签约)
    /// </summary>
    [JsonProperty("channel")]
    public string? Channel { get; set; }
}

/// <summary>
/// JsonProperty Data Structure.
/// </summary>
public class PeriodRuleParams
{
    /// <summary>
    /// 首次执行时间execute_time是周期扣款产品必填，即商户发起首次扣款的时间。精确到日，格式为yyyy-MM-dd 结合其他必填的扣款周期参数，会确定商户以后的扣款计划。发起扣款的时间需符合这里的扣款计划。
    /// </summary>
    [JsonProperty("execute_time")]
    public string? ExecuteTime { get; set; }

    /// <summary>
    /// 周期数period是周期扣款产品必填。与另一参数period_type组合使用确定扣款周期，例如period_type为DAY，period=90，则扣款周期为90天。
    /// </summary>
    [JsonProperty("period")]
    public long Period { get; set; }

    /// <summary>
    /// 周期类型period_type是周期扣款产品必填，枚举值为DAY和MONTH。 DAY即扣款周期按天计，MONTH代表扣款周期按自然月。 与另一参数period组合使用确定扣款周期，例如period_type为DAY，period=30，则扣款周期为30天；period_type为MONTH，period=3，则扣款周期为3个自然月。 自然月是指，不论这个月有多少天，周期都计算到月份中的同一日期。例如1月3日到2月3日为一个自然月，1月3日到4月3日为三个自然月。注意周期类型使用MONTH的时候，计划扣款时间execute_time不允许传28日之后的日期（可以传28日），以此避免有些月份可能不存在对应日期的情况。
    /// </summary>
    [JsonProperty("period_type")]
    public string? PeriodType { get; set; }

    /// <summary>
    /// 单次扣款最大金额single_amount是周期扣款产品必填，即每次发起扣款时限制的最大金额，单位为元。商户每次发起扣款都不允许大于此金额。
    /// </summary>
    [JsonProperty("single_amount")]
    public string? SingleAmount { get; set; }

    /// <summary>
    /// 总金额限制，单位为元。如果传入此参数，商户多次扣款的累计金额不允许超过此金额。
    /// </summary>
    [JsonProperty("total_amount")]
    public string? TotalAmount { get; set; }

    /// <summary>
    /// 总扣款次数。如果传入此参数，则商户成功扣款的次数不能超过此次数限制（扣款失败不计入）。
    /// </summary>
    [JsonProperty("total_payments")]
    public long TotalPayments { get; set; }
}

/// <summary>
/// SignParams Data Structure.
/// </summary>
public class SignParams
{
    /// <summary>
    /// 请按当前接入的方式进行填充，且输入值必须为文档中的参数取值范围。
    /// </summary>
    [JsonProperty("access_params")]
    public AccessParams? AccessParams { get; set; }

    /// <summary>
    /// 是否允许花芝GO降级成原代扣（即销售方案指定的代扣产品），在花芝GO场景下才会使用该值。取值：true-允许降级，false-不允许降级。默认为true。
    /// </summary>
    [JsonProperty("allow_huazhi_degrade")]
    public bool AllowHuazhiDegrade { get; set; }

    /// <summary>
    /// 商户签约号，代扣协议中标示用户的唯一签约号（确保在商户系统中唯一）。 格式规则：支持大写小写字母和数字，最长32位。 商户系统按需传入，如果同一用户在同一产品码、同一签约场景下，签订了多份代扣协议，那么需要指定并传入该值。
    /// </summary>
    [JsonProperty("external_agreement_no")]
    public string? ExternalAgreementNo { get; set; }

    /// <summary>
    /// 用户在商户网站的登录账号，用于在签约页面展示，如果为空，则不展示
    /// </summary>
    [JsonProperty("external_logon_id")]
    public string? ExternalLogonId { get; set; }

    /// <summary>
    /// 周期管控规则参数period_rule_params，在签约周期扣款产品（如CYCLE_PAY_AUTH_P）时必传，在签约其他产品时无需传入。 周期扣款产品，会按照这里传入的参数提示用户，并对发起扣款的时间、金额、次数等做相应限制。
    /// </summary>
    [JsonProperty("period_rule_params")]
    public PeriodRuleParams? PeriodRuleParams { get; set; }

    /// <summary>
    /// 个人签约产品码，商户和支付宝签约时确定。
    /// </summary>
    [JsonProperty("personal_product_code")]
    public string? PersonalProductCode { get; set; }

    /// <summary>
    /// 签约成功后商户用于接收异步通知的地址。如果不传入，签约与支付的异步通知都会发到外层notify_url参数传入的地址；如果外层也未传入，签约与支付的异步通知都会发到商户appid配置的网关地址。
    /// </summary>
    [JsonProperty("sign_notify_url")]
    public string? SignNotifyUrl { get; set; }

    /// <summary>
    /// 协议签约场景，商户和支付宝签约时确定，商户可咨询技术支持。
    /// </summary>
    [JsonProperty("sign_scene")]
    public string? SignScene { get; set; }

    /// <summary>
    /// 此参数用于传递子商户信息，无特殊需求时不用关注。目前商户代扣、海外代扣、淘旅行信用住产品支持传入该参数（在销售方案中“是否允许自定义子商户信息”需要选是）。
    /// </summary>
    [JsonProperty("sub_merchant")]
    public SignMerchantParams? SubMerchant { get; set; }
}

/// <summary>
/// SignMerchantParams Data Structure.
/// </summary>
public class SignMerchantParams
{
    /// <summary>
    /// 子商户的商户id
    /// </summary>
    [JsonProperty("sub_merchant_id")]
    public string? SubMerchantId { get; set; }

    /// <summary>
    /// 子商户的商户名称
    /// </summary>
    [JsonProperty("sub_merchant_name")]
    public string? SubMerchantName { get; set; }

    /// <summary>
    /// 子商户的服务描述
    /// </summary>
    [JsonProperty("sub_merchant_service_description")]
    public string? SubMerchantServiceDescription { get; set; }

    /// <summary>
    /// 子商户的服务名称
    /// </summary>
    [JsonProperty("sub_merchant_service_name")]
    public string? SubMerchantServiceName { get; set; }
}

/// <summary>
/// RoyaltyInfo Data Structure.
/// </summary>
public class RoyaltyInfo
{
    /// <summary>
    /// 分账明细的信息，可以描述多条分账指令，json数组。
    /// </summary>
    [JsonProperty("royalty_detail_infos")]
    public List<RoyaltyDetailInfos>? RoyaltyDetailInfos { get; set; }

    /// <summary>
    /// 分账类型  卖家的分账类型，目前只支持传入ROYALTY（普通分账类型）。
    /// </summary>
    [JsonProperty("royalty_type")]
    public string? RoyaltyType { get; set; }
}

/// <summary>
/// RoyaltyDetailInfos Data Structure.
/// </summary>
public class RoyaltyDetailInfos
{
    /// <summary>
    /// 分账的金额，单位为元
    /// </summary>
    [JsonProperty("amount")]
    public string? Amount { get; set; }

    /// <summary>
    /// 分账的比例，值为20代表按20%的比例分账
    /// </summary>
    [JsonProperty("amount_percentage")]
    public string? AmountPercentage { get; set; }

    /// <summary>
    /// 分账批次号  分账批次号。  目前需要和转入账号类型为bankIndex配合使用。
    /// </summary>
    [JsonProperty("batch_no")]
    public string? BatchNo { get; set; }

    /// <summary>
    /// 分账描述信息
    /// </summary>
    [JsonProperty("desc")]
    public string? Desc { get; set; }

    /// <summary>
    /// 商户分账的外部关联号，用于关联到每一笔分账信息，商户需保证其唯一性。  如果为空，该值则默认为“商户网站唯一订单号+分账序列号”
    /// </summary>
    [JsonProperty("out_relation_id")]
    public string? OutRelationId { get; set; }

    /// <summary>
    /// 分账序列号，表示分账执行的顺序，必须为正整数
    /// </summary>
    [JsonProperty("serial_no")]
    public long SerialNo { get; set; }

    /// <summary>
    /// 如果转入账号类型为userId，本参数为接受分账金额的支付宝账号对应的支付宝唯一用户号。以2088开头的纯16位数字。  &#61548; 如果转入账号类型为bankIndex，本参数为28位的银行编号（商户和支付宝签约时确定）。  如果转入账号类型为storeId，本参数为商户的门店ID。
    /// </summary>
    [JsonProperty("trans_in")]
    public string? TransIn { get; set; }

    /// <summary>
    /// 接受分账金额的账户类型：  &#61548; userId：支付宝账号对应的支付宝唯一用户号。  &#61548; bankIndex：分账到银行账户的银行编号。目前暂时只支持分账到一个银行编号。  storeId：分账到门店对应的银行卡编号。  默认值为userId。
    /// </summary>
    [JsonProperty("trans_in_type")]
    public string? TransInType { get; set; }

    /// <summary>
    /// 如果转出账号类型为userId，本参数为要分账的支付宝账号对应的支付宝唯一用户号。以2088开头的纯16位数字。
    /// </summary>
    [JsonProperty("trans_out")]
    public string? TransOut { get; set; }

    /// <summary>
    /// 要分账的账户类型。  目前只支持userId：支付宝账号对应的支付宝唯一用户号。  默认值为userId。
    /// </summary>
    [JsonProperty("trans_out_type")]
    public string? TransOutType { get; set; }
}

/// <summary>
/// SubMerchant Data Structure.
/// </summary>
public class SubMerchant
{
    /// <summary>
    /// 间连受理商户的支付宝商户编号，通过间连商户入驻后得到。间连业务下必传，并且需要按规范传递受理商户编号。
    /// </summary>
    [JsonProperty("merchant_id")]
    public string? MerchantId { get; set; }

    /// <summary>
    /// 商户id类型，
    /// </summary>
    [JsonProperty("merchant_type")]
    public string? MerchantType { get; set; }
}

/// <summary>
/// SettleInfo Data Structure.
/// </summary>
public class SettleInfo
{
    /// <summary>
    /// 结算详细信息，json数组，目前只支持一条。
    /// </summary>
    [JsonProperty("settle_detail_infos")]
    public List<SettleDetailInfo>? SettleDetailInfos { get; set; }

    /// <summary>
    /// 该笔订单的超期自动确认结算时间，到达期限后，将自动确认结算。此字段只在签约账期结算模式时有效。取值范围：1d～365d。d-天。 该参数数值不接受小数点。
    /// </summary>
    [JsonProperty("settle_period_time")]
    public string? SettlePeriodTime { get; set; }
}

/// <summary>
/// SettleDetailInfo Data Structure.
/// </summary>
public class SettleDetailInfo
{
    /// <summary>
    /// 结算的金额，单位为元。在创建订单和支付接口时必须和交易金额相同。在结算确认接口时必须等于交易金额减去已退款金额。
    /// </summary>
    [JsonProperty("amount")]
    public string? Amount { get; set; }

    /// <summary>
    /// 结算主体标识。当结算主体类型为SecondMerchant时，为二级商户的SecondMerchantID；当结算主体类型为Store时，为门店的外标。
    /// </summary>
    [JsonProperty("settle_entity_id")]
    public string? SettleEntityId { get; set; }

    /// <summary>
    /// 结算主体类型。  二级商户:SecondMerchant;商户或者直连商户门店:Store
    /// </summary>
    [JsonProperty("settle_entity_type")]
    public string? SettleEntityType { get; set; }

    /// <summary>
    /// 结算汇总维度，按照这个维度汇总成批次结算，由商户指定。  目前需要和结算收款方账户类型为cardAliasNo配合使用
    /// </summary>
    [JsonProperty("summary_dimension")]
    public string? SummaryDimension { get; set; }

    /// <summary>
    /// 结算收款方。当结算收款方类型是cardAliasNo时，本参数为用户在支付宝绑定的卡编号；结算收款方类型是userId时，本参数为用户的支付宝账号对应的支付宝唯一用户号，以2088开头的纯16位数字；当结算收款方类型是loginName时，本参数为用户的支付宝登录号；当结算收款方类型是defaultSettle时，本参数不能传值，保持为空。
    /// </summary>
    [JsonProperty("trans_in")]
    public string? TransIn { get; set; }

    /// <summary>
    /// 结算收款方的账户类型。  cardAliasNo：结算收款方的银行卡编号; userId：表示是支付宝账号对应的支付宝唯一用户号; loginName：表示是支付宝登录号； defaultSettle：表示结算到商户进件时设置的默认结算账号，结算主体为门店时不支持传defaultSettle；
    /// </summary>
    [JsonProperty("trans_in_type")]
    public string? TransInType { get; set; }
}

/// <summary>
/// InvoiceInfo Data Structure.
/// </summary>
public class InvoiceInfo
{
    /// <summary>
    /// 开票内容  注：json数组格式
    /// </summary>
    [JsonProperty("details")]
    public string? Details { get; set; }

    /// <summary>
    /// 开票关键信息
    /// </summary>
    [JsonProperty("key_info")]
    public InvoiceKeyInfo? KeyInfo { get; set; }
}

/// <summary>
/// InvoiceKeyInfo Data Structure.
/// </summary>
public class InvoiceKeyInfo
{
    /// <summary>
    /// 开票商户名称：商户品牌简称|商户门店简称
    /// </summary>
    [JsonProperty("invoice_merchant_name")]
    public string? InvoiceMerchantName { get; set; }

    /// <summary>
    /// 该交易是否支持开票
    /// </summary>
    [JsonProperty("is_support_invoice")]
    public bool IsSupportInvoice { get; set; }

    /// <summary>
    /// 税号
    /// </summary>
    [JsonProperty("tax_num")]
    public string? TaxNum { get; set; }
}

/// <summary>
/// AgreementSignParams Data Structure.
/// </summary>
public class AgreementSignParams
{
    /// <summary>
    /// 商户在芝麻端申请的appId
    /// </summary>
    [JsonProperty("buckle_app_id")]
    public string? BuckleAppId { get; set; }

    /// <summary>
    /// 商户在芝麻端申请的merchantId
    /// </summary>
    [JsonProperty("buckle_merchant_id")]
    public string? BuckleMerchantId { get; set; }

    /// <summary>
    /// 商户签约号，代扣协议中标示用户的唯一签约号（确保在商户系统中唯一）。  格式规则：支持大写小写字母和数字，最长32位。  商户系统按需传入，如果同一用户在同一产品码、同一签约场景下，签订了多份代扣协议，那么需要指定并传入该值。
    /// </summary>
    [JsonProperty("external_agreement_no")]
    public string? ExternalAgreementNo { get; set; }

    /// <summary>
    /// 用户在商户网站的登录账号，用于在签约页面展示，如果为空，则不展示
    /// </summary>
    [JsonProperty("external_logon_id")]
    public string? ExternalLogonId { get; set; }

    /// <summary>
    /// 个人签约产品码，商户和支付宝签约时确定。
    /// </summary>
    [JsonProperty("personal_product_code")]
    public string? PersonalProductCode { get; set; }

    /// <summary>
    /// 签约营销参数，此值为json格式；具体的key需与营销约定
    /// </summary>
    [JsonProperty("promo_params")]
    public string? PromoParams { get; set; }

    /// <summary>
    /// 协议签约场景，商户和支付宝签约时确定。  当传入商户签约号external_agreement_no时，场景不能为默认值DEFAULT|DEFAULT。
    /// </summary>
    [JsonProperty("sign_scene")]
    public string? SignScene { get; set; }

    /// <summary>
    /// 当前用户签约请求的协议有效周期。  整形数字加上时间单位的协议有效期，从发起签约请求的时间开始算起。  目前支持的时间单位：  1. d：天  2. m：月  如果未传入，默认为长期有效。
    /// </summary>
    [JsonProperty("sign_validity_period")]
    public string? SignValidityPeriod { get; set; }

    /// <summary>
    /// 签约第三方主体类型。对于三方协议，表示当前用户和哪一类的第三方主体进行签约。  取值范围：  1. PARTNER（平台商户）;  2. MERCHANT（集团商户），集团下子商户可共享用户签约内容;  默认为PARTNER。
    /// </summary>
    [JsonProperty("third_party_type")]
    public string? ThirdPartyType { get; set; }
}

/// <summary>
/// ExtendParams Data Structure.
/// </summary>
public class ExtendParams
{
    /// <summary>
    /// 卡类型
    /// </summary>
    [JsonProperty("card_type")]
    public string? CardType { get; set; }

    /// <summary>
    /// 使用花呗分期要进行的分期数
    /// </summary>
    [JsonProperty("hb_fq_num")]
    public string? HbFqNum { get; set; }

    /// <summary>
    /// 使用花呗分期需要卖家承担的手续费比例的百分值，传入100代表100%
    /// </summary>
    [JsonProperty("hb_fq_seller_percent")]
    public string? HbFqSellerPercent { get; set; }

    /// <summary>
    /// 行业数据回流信息, 详见：地铁支付接口参数补充说明
    /// </summary>
    [JsonProperty("industry_reflux_info")]
    public string? IndustryRefluxInfo { get; set; }

    /// <summary>
    /// 特殊场景下，允许商户指定交易展示的卖家名称
    /// </summary>
    [JsonProperty("specified_seller_name")]
    public string? SpecifiedSellerName { get; set; }

    /// <summary>
    /// 系统商编号  该参数作为系统商返佣数据提取的依据，请填写系统商签约协议的PID
    /// </summary>
    [JsonProperty("sys_service_provider_id")]
    public string? SysServiceProviderId { get; set; }
}

/// <summary>
/// GoodsDetail Data Structure.
/// </summary>
public class GoodsDetail
{
    /// <summary>
    /// 支付宝定义的统一商品编号
    /// </summary>
    [JsonProperty("alipay_goods_id")]
    public string? AlipayGoodsId { get; set; }

    /// <summary>
    /// 商品描述信息
    /// </summary>
    [JsonProperty("body")]
    public string? Body { get; set; }

    /// <summary>
    /// 商品类目树，从商品类目根节点到叶子节点的类目id组成，类目id值使用|分割
    /// </summary>
    [JsonProperty("categories_tree")]
    public string? CategoriesTree { get; set; }

    /// <summary>
    /// 商品类目
    /// </summary>
    [JsonProperty("goods_category")]
    public string? GoodsCategory { get; set; }

    /// <summary>
    /// 商品的编号
    /// </summary>
    [JsonProperty("goods_id")]
    public string? GoodsId { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    [JsonProperty("goods_name")]
    public string? GoodsName { get; set; }

    /// <summary>
    /// 商品单价，单位为元
    /// </summary>
    [JsonProperty("price")]
    public string? Price { get; set; }

    /// <summary>
    /// 商品数量
    /// </summary>
    [JsonProperty("quantity")]
    public long Quantity { get; set; }

    /// <summary>
    /// 商品的展示地址
    /// </summary>
    [JsonProperty("show_url")]
    public string? ShowUrl { get; set; }
}

/// <summary>
/// ExtUserInfo Data Structure.
/// </summary>
public class ExtUserInfo
{
    /// <summary>
    /// 证件号 注：need_check_info=T时该参数才有效
    /// </summary>
    [JsonProperty("cert_no")]
    public string? CertNo { get; set; }

    /// <summary>
    /// 身份证：IDENTITY_CARD、护照：PASSPORT、军官证：OFFICER_CARD、士兵证：SOLDIER_CARD、户口本：HOKOU等。如有其它类型需要支持，请与蚂蚁金服工作人员联系。    注： need_check_info=T时该参数才有效
    /// </summary>
    [JsonProperty("cert_type")]
    public string? CertType { get; set; }

    /// <summary>
    /// 是否强制校验付款人身份信息  T:强制校验，F：不强制
    /// </summary>
    [JsonProperty("fix_buyer")]
    public string? FixBuyer { get; set; }

    /// <summary>
    /// 允许的最小买家年龄，买家年龄必须大于等于所传数值   注：  1. need_check_info=T时该参数才有效  2. min_age为整数，必须大于等于0
    /// </summary>
    [JsonProperty("min_age")]
    public string? MinAge { get; set; }

    /// <summary>
    /// 手机号  注：该参数暂不校验
    /// </summary>
    [JsonProperty("mobile")]
    public string? Mobile { get; set; }

    /// <summary>
    /// 姓名    注： need_check_info=T时该参数才有效
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 是否强制校验身份信息  T:强制校验，F：不强制
    /// </summary>
    [JsonProperty("need_check_info")]
    public string? NeedCheckInfo { get; set; }
}
#endregion
