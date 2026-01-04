using Mapster;
using QT.CMS.Entitys.Dto.Shop;

namespace QT.CMS.Entitys.Profiles.Shop;

/// <summary>
/// 优惠券实体映射
/// </summary>
public class ShopCouponProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        //优惠券，将源数据映射到DTO
        config.ForType<ShopCoupon, ShopCouponDto>();
        config.ForType<ShopCoupon, ShopCouponEditDto>();
        config.ForType<ShopCouponGoodsRelation, ShopCouponGoodsRelationDto>();
        config.ForType<ShopCouponCategoryRelation, ShopCouponCategoryRelationDto>();
        //优惠券，将DTO映射到源数据
        config.ForType<ShopCouponEditDto, ShopCoupon>();
        config.ForType<ShopCouponGoodsRelationDto, ShopCouponGoodsRelation>();
        config.ForType<ShopCouponCategoryRelationDto, ShopCouponCategoryRelation>();

        //优惠券记录，将源数据映射到DTO
        config.ForType<ShopCouponHistory, ShopCouponHistoryDto>();
        config.ForType<ShopCouponHistory, ShopCouponHistoryEditDto>();
        //优惠券，将DTO映射到源数据
        config.ForType<ShopCouponHistoryEditDto, ShopCouponHistory>();
    }
}
