using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 优惠券
/// </summary>
[SugarTable("cms_shop_coupon")]
[Tenant(ClaimConst.TENANTID)]
public class ShopCoupon
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    public int SiteId { get; set; }

    /// <summary>
    /// 优惠券名称
    /// </summary>
    [Display(Name = "优惠券名称")]
    [StringLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// 适用范围0全场1分类2商品
    /// </summary>
    [Display(Name = "适用范围")]
    public byte UseType { get; set; } = 0;

    /// <summary>
    /// 优惠券金额
    /// </summary>
    [Display(Name = "优惠券面额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; } = 0M;

    /// <summary>
    /// 使用门槛0不限制
    /// </summary>
    [Display(Name = "使用门槛")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal MinAmount { get; set; } = 0M;

    /// <summary>
    /// 兑换积分
    /// </summary>
    [Display(Name = "兑换积分")]
    public int Point { get; set; } = 0;

    /// <summary>
    /// 发行数量
    /// </summary>
    [Display(Name = "发行数量")]
    public int PublishCount { get; set; }

    /// <summary>
    /// 已使用数量
    /// </summary>
    [Display(Name = "已使用数量")]
    public int UseCount { get; set; } = 0;

    /// <summary>
    /// 已领取数量
    /// </summary>
    [Display(Name = "已领取数量")]
    public int ReceiveCount { get; set; } = 0;

    /// <summary>
    /// 领取时间
    /// </summary>
    [Display(Name = "领取时间")]
    public DateTime EnableTime { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [Display(Name = "开始时间")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [Display(Name = "结束时间")]
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(30)]
    public string? AddBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    [StringLength(30)]
    public string? UpdateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 商品关联列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ShopCouponGoodsRelation.CouponId))]
    public List<ShopCouponGoodsRelation> GoodsRelations { get; set; } 

    /// <summary>
    /// 分类关联列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ShopCouponCategoryRelation.CouponId))]
    public List<ShopCouponCategoryRelation> CategoryRelations { get; set; }

}
