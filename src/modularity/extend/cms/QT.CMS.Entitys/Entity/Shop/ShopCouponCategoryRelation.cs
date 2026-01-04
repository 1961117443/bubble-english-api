using QT.Common.Const;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 优惠券分类关联
/// </summary>
[SugarTable("cms_shop_coupon_category_relation")]
[Tenant(ClaimConst.TENANTID)]
public class ShopCouponCategoryRelation
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属分类ID
    /// </summary>
    [Display(Name = "所属分类")]
    [ForeignKey("ShopCategory")]
    public long CategoryId { get; set; }

    /// <summary>
    /// 所属优惠券ID
    /// </summary>
    [Display(Name = "所属优惠券")]
    [ForeignKey("ShopCoupon")]
    public long CouponId { get; set; }

    /// <summary>
    /// 分类信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(CategoryId))]
    public ShopCategory? ShopCategory { get; set; }

    /// <summary>
    /// 优惠券信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(CouponId))]
    public ShopCoupon? ShopCoupon { get; set; }
}
