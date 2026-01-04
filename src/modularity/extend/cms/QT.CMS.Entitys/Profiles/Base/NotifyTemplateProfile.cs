using Mapster;
using QT.CMS.Entitys.Dto.Base;

namespace QT.CMS.Entitys.Profiles.Base;

public class NotifyTemplateProfile : IRegister
{
    /// <summary>
    /// 通知模板实体映射
    /// </summary>
    public void Register(TypeAdapterConfig config)
    {
        //将源数据映射到DTO
        config.ForType<NotifyTemplate, NotifyTemplateDto>();
        config.ForType<NotifyTemplate, NotifyTemplateEditDto>();
        //将DTO映射到源数据
        config.ForType<NotifyTemplateEditDto, NotifyTemplate>();
    }
}
