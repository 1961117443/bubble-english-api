using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 优惠券分类关联
/// </summary>
public class ShopCouponCategoryRelationDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 所属分类ID
    /// </summary>
    [Display(Name = "所属分类")]
    public long categoryId { get; set; }

    /// <summary>
    /// 所属优惠券
    /// </summary>
    [Display(Name = "所属优惠券")]
    public long couponId { get; set; }
}
