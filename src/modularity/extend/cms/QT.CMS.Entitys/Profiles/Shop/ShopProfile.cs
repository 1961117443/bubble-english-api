using Mapster;
using QT.CMS.Entitys.Dto.Shop;
namespace QT.CMS.Entitys.Profiles.Shop;

/// <summary>
/// 商城实体映射
/// </summary>
public class ShopProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        //商城类别，将源数据映射到DTO
        config.ForType<ShopCategory, ShopCategoryDto>();
        config.ForType<ShopCategory, ShopCategoryEditDto>();
        config.ForType<ShopCategory, ShopCategoryListDto>();
        //商城类别，将DTO映射到源数据
        config.ForType<ShopCategoryEditDto, ShopCategory>();

        //商品标签，将源数据映射到DTO
        config.ForType<ShopLabel, ShopLabelDto>();
        config.ForType<ShopLabel, ShopLabelEditDto>();
        //商品标签，将DTO映射到源数据
        config.ForType<ShopLabelEditDto, ShopLabel>();

        //商品品牌，将源数据映射到DTO
        config.ForType<ShopBrand, ShopBrandDto>();
        config.ForType<ShopBrand, ShopBrandEditDto>();
        //商品品牌，将DTO映射到源数据
        config.ForType<ShopBrandEditDto, ShopBrand>();

        //商品规格，将源数据映射到DTO
        config.ForType<ShopSpec, ShopSpecDto>();
        config.ForType<ShopSpec, ShopSpecListDto>();
        config.ForType<ShopSpec, ShopSpecEditDto>();
        config.ForType<ShopSpec, ShopSpecChildrenDto>();
        //商品规格，将DTO映射到源数据
        config.ForType<ShopSpecEditDto, ShopSpec>();
        config.ForType<ShopSpecChildrenDto, ShopSpec>();

        //快递公司，将源数据映射到DTO
        config.ForType<ShopExpress, ShopExpressDto>();
        config.ForType<ShopExpress, ShopExpressEditDto>();
        //快递公司，将DTO映射到源数据
        config.ForType<ShopExpressEditDto, ShopExpress>();

        //配送方式，将源数据映射到DTO
        config.ForType<ShopDelivery, ShopDeliveryDto>();
        config.ForType<ShopDelivery, ShopDeliveryEditDto>();
        config.ForType<ShopDeliveryArea, ShopDeliveryAreaDto>();
        //配送方式，将DTO映射到源数据
        config.ForType<ShopDeliveryEditDto, ShopDelivery>();
        config.ForType<ShopDeliveryAreaDto, ShopDeliveryArea>();

        //扩展属性，将源数据映射到DTO
        config.ForType<ShopField, ShopFieldDto>();
        config.ForType<ShopField, ShopFieldEditDto>();
        config.ForType<ShopFieldOption, ShopFieldOptionDto>();
        //扩展属性，将DTO映射到源数据
        config.ForType<ShopFieldEditDto, ShopField>();
        config.ForType<ShopFieldOptionDto, ShopFieldOption>();

        //商品收藏，将源数据映射到DTO
        config.ForType<ShopGoodsFavorite, ShopGoodsFavoriteDto>();
        //商品收藏，将DTO映射到源数据
        config.ForType<ShopGoodsFavoriteAddDto, ShopGoodsFavorite>();

        //促销活动，将源数据映射到DTO
        config.ForType<ShopPromotion, ShopPromotionDto>();
        config.ForType<ShopPromotion, ShopPromotionEditDto>();
        //促销活动，将DTO映射到源数据
        config.ForType<ShopPromotionEditDto, ShopPromotion>();

        //商品抢购，将源数据映射到DTO
        config.ForType<ShopSpeed, ShopSpeedDto>();
        config.ForType<ShopSpeed, ShopSpeedEditDto>();
        //商品抢购，将DTO映射到源数据
        config.ForType<ShopSpeedEditDto, ShopSpeed>();

        //购物车，将源数据映射到DTO
        config.ForType<ShopCart, ShopCartDto>();
    }
}
