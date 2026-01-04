using Mapster;
using QT.CMS.Entitys.Dto.Order;

namespace QT.CMS.Entitys.Profiles.Order;


/// <summary>
/// 订单实体映射
/// </summary>
public class OrderProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        //订单信息，将源数据映射到DTO
        config.ForType<Orders, OrdersDto>()
            .Map(
                dest => dest.tradeNo,
               src => src.Collection != null ? src.Collection.TradeNo : null
            ).Map(
                dest => dest.paymentId,
               src => src.Collection != null ? src.Collection.PaymentId : 0
            ).Map(
                dest => dest.paymentTitle,
               src => src.Collection != null ? src.Collection.PaymentTitle : null
            );
        config.ForType<Orders, OrdersEditDto>();
        config.ForType<OrderGoods, OrderGoodsDto>();
        config.ForType<OrderPromotion, OrderPromotionDto>()
            .Map(
                dest => dest.orderNo,
               src => src.Order != null ? src.Order.OrderNo : null
            ).Map(
                dest => dest.promotionTitle,
               src => src.Promotion != null ? src.Promotion.Title : null
            );
        config.ForType<OrderLog, OrderLogDto>();
        //订单信息，将DTO映射到源数据
        config.ForType<OrdersEditDto, Orders>();
        config.ForType<OrderGoodsEditDto, OrderGoods>();
        config.ForType<OrderPromotionEditDto, OrderPromotion>();
        config.ForType<OrderLogDto, OrderLog>();

        //订单发货，将源数据映射到DTO
        config.ForType<OrderDelivery, OrderDeliveryDto>();
        config.ForType<OrderDelivery, OrderDeliveryEditDto>();
        config.ForType<OrderDeliveryGoods, OrderDeliveryGoodsDto>();
        //订单发货，将DTO映射到源数据
        config.ForType<OrderDeliveryEditDto, OrderDelivery>();
        config.ForType<OrderDeliveryGoodsDto, OrderDeliveryGoods>();

        //订单评价，将源数据映射到DTO
        //config.ForType<OrderEvaluate, OrderEvaluateDto>()
        //    .Map(
        //        dest => dest.Member,
        //       src => src.User != null ? src.User.Member : null
        //    );
        config.ForType<OrderEvaluate, OrderEvaluateEditDto>();
        config.ForType<OrderEvaluateAlbum, OrderEvaluateAlbumDto>();
        //订单评价，将DTO映射到源数据
        config.ForType<OrderEvaluateEditDto, OrderEvaluate>();
        config.ForType<OrderEvaluateAlbumDto, OrderEvaluateAlbum>();

        //退换货，将源数据映射到DTO
        config.ForType<OrderRefund, OrderRefundDto>();
        config.ForType<OrderRefundGoods, OrderRefundGoodsDto>();
        config.ForType<OrderRefundGoods, OrderRefundGoodsEditDto>();
        config.ForType<OrderRefundAlbum, OrderRefundAlbumDto>();
        //退换货，将DTO映射到源数据
        config.ForType<OrderRefundApplyDto, OrderRefund>();
        config.ForType<OrderRefundBuyDto, OrderRefund>();
        config.ForType<OrderRefundSellerDto, OrderRefund>();
        config.ForType<OrderRefundHandleDto, OrderRefund>();
        config.ForType<OrderRefundGoodsEditDto, OrderRefundGoods>();
        config.ForType<OrderRefundAlbumDto, OrderRefundAlbum>();
    }
}
