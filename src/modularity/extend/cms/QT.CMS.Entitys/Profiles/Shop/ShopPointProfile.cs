using Mapster;
using QT.CMS.Entitys.Dto.Shop;

namespace QT.CMS.Entitys.Profiles.Shop;

/// <summary>
/// 积分实体映射
/// </summary>
public class ShopPointProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        //积分兑换，将源数据映射到DTO
        config.ForType<ShopConvert, ShopConvertDto>();
        config.ForType<ShopConvert, ShopConvertEditDto>();
        config.ForType<ShopConvertHistory, ShopConvertHistoryDto>()
            .Map(
                dest => dest.userName,
                src => src.User != null ? src.User.RealName : null
            ).Map(
                dest => dest.convertTitle,
                src => src.ShopConvert != null ? src.ShopConvert.Title : null
            ).Map(
                dest => dest.orderNo,
                src => src.Order != null ? src.Order.OrderNo : null
            );
        //积分兑换，将DTO映射到源数据
        config.ForType<ShopConvertEditDto, ShopConvert>();
        config.ForType<ShopConvertHistoryDto, ShopConvertHistory>();
    }
}
