using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 优惠券商品关联
/// </summary>
[SugarTable("cms_shop_coupon_goods_relation")]
[Tenant(ClaimConst.TENANTID)]
public class ShopCouponGoodsRelation
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属商品
    /// </summary>
    [Display(Name = "所属商品")]
    [ForeignKey("ShopGoods")]
    public long GoodsId { get; set; }

    /// <summary>
    /// 所属优惠券
    /// </summary>
    [Display(Name = "所属优惠券")]
    [ForeignKey("ShopCoupon")]
    public long CouponId { get; set; }

    /// <summary>
    /// 商品信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(GoodsId))]
    public ShopGoods? ShopGoods { get; set; }

    /// <summary>
    /// 优惠券信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(CouponId))]
    public ShopCoupon? ShopCoupon { get; set; }
}
