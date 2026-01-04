using Mapster;
using QT.CMS.Entitys.Dto.Weixin;

namespace QT.CMS.Entitys.Profiles.Weixin;

public class WeixinProfile : IRegister
{
    /// <summary>
    /// 微信公众号实体映射
    /// </summary>
    public void Register(TypeAdapterConfig config)
    {
        //将源数据映射到DTO
        config.ForType<WxAccount, WxAccountDto>();
        config.ForType<WxAccount, WxAccountEditDto>();
        //将DTO映射到源数据
        config.ForType<WxAccountEditDto, WxAccount>();
    }
}
