using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 优惠券商品关联
/// </summary>
public class ShopCouponGoodsRelationDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 所属商品ID
    /// </summary>
    [Display(Name = "所属商品")]
    public long goodsId { get; set; }

    /// <summary>
    /// 所属优惠券ID
    /// </summary>
    [Display(Name = "所属优惠券")]
    public long couponId { get; set; }

    /// <summary>
    /// 商品信息
    /// </summary>
    public ShopGoodsDto? shopGoods { get; set; }
}
