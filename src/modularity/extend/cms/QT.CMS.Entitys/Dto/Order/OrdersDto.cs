using QT.CMS.Entitys.Dto.Shop;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys.Dto.Order;

/// <summary>
/// 订单(显示)
/// </summary>
public class OrdersDto : OrdersBaseDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public string id { get; set; }

    /// <summary>
    /// 交易单号
    /// </summary>
    [Display(Name = "交易单号")]
    public string? tradeNo { get; set; }

    /// <summary>
    /// 支付方式名称
    /// </summary>
    [Display(Name = "支付方式名称")]
    public string? paymentTitle { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    [Display(Name = "订单号")]
    public string? orderNo { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    public string? userName { get; set; }

    /// <summary>
    /// 下单时间
    /// </summary>
    [Display(Name = "下单时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 付款时间
    /// </summary>
    [Display(Name = "付款时间")]
    public DateTime? paymentTime { get; set; }

    /// <summary>
    /// 发货时间
    /// </summary>
    [Display(Name = "发货时间")]
    public DateTime? deliveryTime { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    [Display(Name = "完成时间")]
    public DateTime? completeTime { get; set; }

    /// <summary>
    /// 增加经验
    /// </summary>
    [Display(Name = "增加经验")]
    public int exp { get; set; } = 0;

    /// <summary>
    /// 增加/兑换积分
    /// </summary>
    [Display(Name = "增加积分")]
    public int point { get; set; } = 0;

    /// <summary>
    /// 应付总运费
    /// </summary>
    [Display(Name = "应付总运费")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal payableFreight { get; set; } = 0M;

    /// <summary>
    /// 应付商品总金额
    /// </summary>
    [Display(Name = "应付商品总金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal payableAmount { get; set; } = 0M;

    /// <summary>
    /// 实付商品总金额
    /// </summary>
    [Display(Name = "实付商品总金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal realAmount { get; set; } = 0M;

    /// <summary>
    /// 优惠券金额
    /// </summary>
    [Display(Name = "优惠券金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal couponAmount { get; set; } = 0M;

    /// <summary>
    /// 促销优惠金额
    /// </summary>
    [Display(Name = "促销优惠金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal promotionAmount { get; set; } = 0M;

    /// <summary>
    /// 订单总金额
    /// </summary>
    [Display(Name = "订单总金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal orderAmount { get; set; } = 0M;

    /// <summary>
    /// 0未支付
    /// 1已支付
    /// </summary>
    [Display(Name = "支付状态")]
    public byte paymentStatus { get; set; } = 0;

    /// <summary>
    /// 0未发货
    /// 1全部发货
    /// 2部分发货
    /// </summary>
    [Display(Name = "配送状态")]
    public byte deliveryStatus { get; set; } = 0;

    /// <summary>
    /// 退款状态
    /// 0无退款
    /// 1全部退款
    /// 2部分退款
    /// </summary>
    [Display(Name = "退款状态")]
    public byte refundStatus { get; set; } = 0;

    /// <summary>
    /// 1待付款
    /// 2待发货
    /// 3待收货
    /// 4已签收
    /// 5完成
    /// 6取消(客户)
    /// 7作废(管理员)
    /// </summary>
    [Display(Name = "订单状态")]
    public byte status { get; set; } = 0;

    /// <summary>
    /// 订单商品列表
    /// </summary>
    public ICollection<OrderGoodsDto> orderGoods { get; set; } = new List<OrderGoodsDto>();

    /// <summary>
    /// 订单促销活动关联列表
    /// </summary>
    public ICollection<OrderPromotionDto> orderPromotion { get; set; } = new List<OrderPromotionDto>();

    /// <summary>
    /// 订单日志列表
    /// </summary>
    public ICollection<OrderLogDto> orderLog { get; set; } = new List<OrderLogDto>();

    /// <summary>
    /// 配送方式
    /// </summary>
    public ShopDeliveryDto? shopDelivery { get; set; }
}

/// <summary>
/// 订单(编辑)
/// </summary>
public class OrdersEditDto : OrdersBaseDto
{
    /// <summary>
    /// 使用优惠券ID
    /// </summary>
    public long? useCouponId { get; set; } = 0;

    /// <summary>
    /// 订单商品列表
    /// </summary>
    public ICollection<OrderGoodsEditDto> orderGoods { get; set; } = new List<OrderGoodsEditDto>();
}

/// <summary>
/// 订单(公共)
/// </summary>
public class OrdersBaseDto
{
    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int siteId { get; set; }

    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    public string? userId { get; set; } = "";

    /// <summary>
    /// 0普通订单
    /// 1抢购订单
    /// 2积分换购
    /// </summary>
    [Display(Name = "订单类型")]
    [Required(ErrorMessage = "{0}不可为空")]
    public byte orderType { get; set; } = 0;

    /// <summary>
    /// OrderType=0时促销活动表ID
    /// OrderType=1时抢购表ID
    /// OrderType=2时积分换购表ID
    /// </summary>
    [Display(Name = "所属活动")]
    public long activeId { get; set; } = 0;

    /// <summary>
    /// 支付方式ID
    /// </summary>
    [Display(Name = "支付方式")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int paymentId { get; set; } = 0;

    /// <summary>
    /// 配送方式ID
    /// </summary>
    [Display(Name = "配送方式")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int deliveryId { get; set; }

    /// <summary>
    /// 收货人
    /// </summary>
    [Display(Name = "收货人")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(30)]
    public string? acceptName { get; set; }

    /// <summary>
    /// 固定电话
    /// </summary>
    [Display(Name = "固定电话")]
    [StringLength(30)]
    public string? telPhone { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    [Display(Name = "手机号码")]
    [Required(ErrorMessage = "{0}不可为空")]
    [RegularExpression(@"^(13|14|15|16|18|19|17)\d{9}$", ErrorMessage = "{0}格式有误")]
    [StringLength(30)]
    public string? mobile { get; set; }

    /// <summary>
    /// 所属省份
    /// </summary>
    [Display(Name = "所属省份")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? province { get; set; }

    /// <summary>
    /// 所属城市
    /// </summary>
    [Display(Name = "所属城市")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? city { get; set; }

    /// <summary>
    /// 所属地区
    /// </summary>
    [Display(Name = "所属地区")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? area { get; set; }

    /// <summary>
    /// 详细地址
    /// </summary>
    [Display(Name = "详细地址")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(512)]
    public string? address { get; set; }

    /// <summary>
    /// 实付总运费
    /// </summary>
    [Display(Name = "实付总运费")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? realFreight { get; set; }

    /// <summary>
    /// 订单折扣或涨价
    /// </summary>
    [Display(Name = "订单折扣或涨价")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal discountAmount { get; set; } = 0M;

    /// <summary>
    /// 是否保价
    /// </summary>
    [Display(Name = "是否保价")]
    public byte isInsure { get; set; } = 0;

    /// <summary>
    /// 保价金额
    /// </summary>
    [Display(Name = "保价金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? insurePrice { get; set; } = 0M;

    /// <summary>
    /// 用户留言
    /// </summary>
    [Display(Name = "用户留言")]
    [StringLength(512)]
    public string? postscript { get; set; }

    /// <summary>
    /// 管理员备注
    /// </summary>
    [Display(Name = "管理员备注")]
    [StringLength(512)]
    public string? remark { get; set; }

    /// <summary>
    /// 收货时间说明
    /// </summary>
    [Display(Name = "收货时间说明")]
    [StringLength(128)]
    public string? acceptTime { get; set; }
}
