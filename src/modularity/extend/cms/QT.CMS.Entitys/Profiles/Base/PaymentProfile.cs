using Mapster;
using QT.CMS.Entitys.Dto.Base;

namespace QT.CMS.Entitys.Profiles.Base;

public class PaymentProfile : IRegister
{
    /// <summary>
    /// 支付方式实体映射
    /// </summary>
    public void Register(TypeAdapterConfig config)
    {
        //支付方式，将源数据映射到DTO
        config.ForType<Payments, PaymentsDto>();
        config.ForType<Payments, PaymentsEditDto>();
        //支付方式，将DTO映射到源数据
        config.ForType<PaymentsEditDto, Payments>();

        //支付收款，将源数据映射到DTO
        config.ForType<PaymentCollection, PaymentCollectionDto>();
        //支付收款，将DTO映射到源数据
        config.ForType<PaymentCollectionEditDto, PaymentCollection>();

        //站点支付方式，将源数据映射到DTO
        config.ForType<SitePayment, SitePaymentDto>();
        config.ForType<SitePayment, SitePaymentEditDto>();
        //站点支付方式，将DTO映射到源数据
        config.ForType<SitePaymentEditDto, SitePayment>();
    }
}
