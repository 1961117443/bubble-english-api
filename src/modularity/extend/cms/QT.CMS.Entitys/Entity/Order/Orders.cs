using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 订单
/// </summary>
[SugarTable("cms_orders")]
public class Orders
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属站点
    /// </summary>
    [Display(Name = "所属站点")]
    public int SiteId { get; set; }

    /// <summary>
    /// 0普通订单
    /// 1抢购订单
    /// 2积分换购
    /// </summary>
    [Display(Name = "订单类型")]
    public byte OrderType { get; set; } = 0;

    /// <summary>
    /// OrderType=0时促销活动表ID
    /// OrderType=1时抢购表ID
    /// OrderType=2时积分换购表ID
    /// </summary>
    [Display(Name = "所属活动")]
    public long ActiveId { get; set; } = 0;

    /// <summary>
    /// 订单号
    /// </summary>
    [Display(Name = "订单号")]
    [Required]
    [StringLength(128)]
    public string? OrderNo { get; set; }

    /// <summary>
    /// 所属用户
    /// </summary>
    [Display(Name = "所属用户")]
    public string? UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [StringLength(128)]
    public string? UserName { get; set; }

    /// <summary>
    /// 支付收款ID
    /// </summary>
    [Display(Name = "支付收款")]
    [ForeignKey("Collection")]
    public long CollectionId { get; set; }

    /// <summary>
    /// 配送方式ID
    /// </summary>
    [Display(Name = "配送方式")]
    [ForeignKey("ShopDelivery")]
    public int DeliveryId { get; set; }

    /// <summary>
    /// 收货人
    /// </summary>
    [Display(Name = "收货人")]
    [StringLength(30)]
    public string? AcceptName { get; set; }

    /// <summary>
    /// 固定电话
    /// </summary>
    [Display(Name = "固定电话")]
    [StringLength(30)]
    public string? TelPhone { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    [Display(Name = "手机号码")]
    [StringLength(30)]
    public string? Mobile { get; set; }

    /// <summary>
    /// 所属省份
    /// </summary>
    [Display(Name = "所属省份")]
    [StringLength(30)]
    public string? Province { get; set; }

    /// <summary>
    /// 所属城市
    /// </summary>
    [Display(Name = "所属城市")]
    [StringLength(30)]
    public string? City { get; set; }

    /// <summary>
    /// 所属地区
    /// </summary>
    [Display(Name = "所属地区")]
    [StringLength(30)]
    public string? Area { get; set; }

    /// <summary>
    /// 详细地址
    /// </summary>
    [Display(Name = "详细地址")]
    [StringLength(512)]
    public string? Address { get; set; }

    /// <summary>
    /// 应付总运费
    /// </summary>
    [Display(Name = "应付总运费")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal PayableFreight { get; set; } = 0M;

    /// <summary>
    /// 实付总运费
    /// </summary>
    [Display(Name = "实付总运费")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal RealFreight { get; set; } = 0M;

    /// <summary>
    /// 应付商品总金额
    /// </summary>
    [Display(Name = "应付商品总金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal PayableAmount { get; set; } = 0M;

    /// <summary>
    /// 实付商品总金额
    /// </summary>
    [Display(Name = "实付商品总金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal RealAmount { get; set; } = 0M;

    /// <summary>
    /// 优惠券金额
    /// </summary>
    [Display(Name = "优惠券金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal CouponAmount { get; set; } = 0M;

    /// <summary>
    /// 促销优惠金额
    /// </summary>
    [Display(Name = "促销优惠金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal PromotionAmount { get; set; } = 0M;

    /// <summary>
    /// 订单折扣或涨价
    /// </summary>
    [Display(Name = "订单折扣或涨价")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal DiscountAmount { get; set; } = 0M;

    /// <summary>
    /// 订单总金额
    /// </summary>
    [Display(Name = "订单总金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal OrderAmount { get; set; } = 0M;

    /// <summary>
    /// 是否保价
    /// </summary>
    [Display(Name = "是否保价")]
    public byte IsInsure { get; set; } = 0;

    /// <summary>
    /// 保价金额
    /// </summary>
    [Display(Name = "保价金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal InsurePrice { get; set; } = 0M;

    /// <summary>
    /// 0未支付
    /// 1已支付
    /// </summary>
    [Display(Name = "支付状态")]
    public byte PaymentStatus { get; set; } = 0;

    /// <summary>
    /// 0未发货
    /// 1全部发货
    /// 2部分发货
    /// </summary>
    [Display(Name = "配送状态")]
    public byte DeliveryStatus { get; set; } = 0;

    /// <summary>
    /// 退款状态
    /// 0无退款
    /// 1全部退款
    /// 2部分退款
    /// </summary>
    [Display(Name = "退款状态")]
    public byte RefundStatus { get; set; } = 0;

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
    public byte Status { get; set; } = 1;

    /// <summary>
    /// 用户留言
    /// </summary>
    [Display(Name = "用户留言")]
    [StringLength(512)]
    public string? Postscript { get; set; }

    /// <summary>
    /// 管理员备注
    /// </summary>
    [Display(Name = "管理员备注")]
    [StringLength(512)]
    public string? Remark { get; set; }

    /// <summary>
    /// 增加经验
    /// </summary>
    [Display(Name = "增加经验")]
    public int Exp { get; set; } = 0;

    /// <summary>
    /// 增加/兑换积分
    /// </summary>
    [Display(Name = "增加积分")]
    public int Point { get; set; } = 0;

    /// <summary>
    /// 收货时间说明
    /// </summary>
    [Display(Name = "收货时间说明")]
    [StringLength(128)]
    public string? AcceptTime { get; set; }

    /// <summary>
    /// 下单时间
    /// </summary>
    [Display(Name = "下单时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 付款时间
    /// </summary>
    [Display(Name = "付款时间")]
    public DateTime? PaymentTime { get; set; }

    /// <summary>
    /// 发货时间
    /// </summary>
    [Display(Name = "发货时间")]
    public DateTime? DeliveryTime { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    [Display(Name = "完成时间")]
    public DateTime? CompleteTime { get; set; }


    /// <summary>
    /// 订单商品列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(QT.CMS.Entitys.OrderGoods.OrderId), nameof(Id))]
    public List<OrderGoods> OrderGoods { get; set; }

    /// <summary>
    /// 订单促销活动列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(QT.CMS.Entitys.OrderPromotion.OrderId), nameof(Id))]
    public List<OrderPromotion> OrderPromotion { get; set; }

    /// <summary>
    /// 订单日志列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(QT.CMS.Entitys.OrderLog.OrderId), nameof(Id))]
    public List<OrderLog> OrderLog { get; set; }

    /// <summary>
    /// 支付收款
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(CollectionId))]
    public PaymentCollection? Collection { get; set; }

    /// <summary>
    /// 配送方式
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(DeliveryId))]
    public ShopDelivery? ShopDelivery { get; set; }
}
