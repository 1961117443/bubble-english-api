using Mapster;
using QT.CMS.Entitys.Dto.Login;
using QT.CMS.Entitys.Dto.Member;

namespace QT.CMS.Entitys.Profiles.Member;

/// <summary>
/// 会员实体映射
/// </summary>
public class MemberProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        //会员信息，将源数据映射到DTO
        config.ForType<Members, MembersDto>()
            .Map(
                dest => dest.groupTitle,
                src => src.Group != null ? src.Group.Title : null
            ).Map(
                dest => dest.userName,
                src => src.User != null ? src.User.Account : null
            ).Map(
                dest => dest.email,
                src => src.User != null ? src.User.Email : null
            ).Map(
                dest => dest.phone,
                src => src.User != null ? src.User.MobilePhone : null
            ).Map(
                dest => dest.status,
                src => src.User != null ? (src.User.EnabledMark == 1 ? 0 : (src.User.EnabledMark == 2?3: src.User.EnabledMark == 3? 2:(src.User.EnabledMark == 0? 3 :0))) : 0
            );
        config.ForType<Members, MembersEditDto>();
        //会员信息，将源数据映射到DTO
        config.ForType<MembersEditDto, Members>();
        config.ForType<MembersModifyDto, Members>();
        config.ForType<RegisterDto, MembersEditDto>();

        //会员组，将源数据映射到DTO
        config.ForType<MemberGroup, MemberGroupDto>();
        config.ForType<MemberGroup, MemberGroupEditDto>();
        //会员组，将DTO映射到源数据
        config.ForType<MemberGroupEditDto, MemberGroup>();

        //收货地址，将源数据映射到DTO
        config.ForType<MemberAddress, MemberAddressDto>();
        config.ForType<MemberAddress, MemberAddressEditDto>();
        //收货地址，将DTO映射到源数据
        config.ForType<MemberAddressEditDto, MemberAddress>();

        //站内消息，将源数据映射到DTO
        config.ForType<MemberMessage, MemberMessageEditDto>();
        //站内消息，将DTO映射到源数据
        config.ForType<MemberMessageEditDto, MemberMessage>();

        //会员充值，将源数据映射到DTO
        config.ForType<MemberRecharge, MemberRechargeDto>()
            .Map(
                dest => dest.tradeNo,
                src => src.Collection != null ? src.Collection.TradeNo : null
            ).Map(
                dest => dest.paymentTitle,
                src => src.Collection != null ? src.Collection.PaymentTitle : null
            ).Map(
                dest => dest.status,
                src => src.Collection != null ? src.Collection.Status : 1
            ).Map(
                dest => dest.addTime,
                src => src.Collection != null ? src.Collection.AddTime : DateTime.Now
            ).Map(
                dest => dest.completeTime,
                src => src.Collection != null ? src.Collection.CompleteTime : null
            );
        config.ForType<MemberRecharge, MemberRechargeEditDto>();
        //会员充值，将DTO映射到源数据
        config.ForType<MemberRechargeEditDto, MemberRecharge>();

        //积分记录，将源数据映射到DTO
        config.ForType<MemberPointLog, MemberPointLogEditDto>();
        //积分记录，将DTO映射到源数据
        config.ForType<MemberPointLogEditDto, MemberPointLog>();

        //余额记录，将源数据映射到DTO
        config.ForType<MemberAmountLog, MemberAmountLogDto>();
        config.ForType<MemberAmountLog, MemberAmountLogEditDto>();
        //余额记录，将DTO映射到源数据
        config.ForType<MemberAmountLogEditDto, MemberAmountLog>();

        //附件下载记录，将源数据映射到DTO
        config.ForType<MemberAttachLog, MemberAttachLogEditDto>();
        //附件下载记录，将DTO映射到源数据
        config.ForType<MemberAttachLogEditDto, MemberAttachLog>();
    }
}
