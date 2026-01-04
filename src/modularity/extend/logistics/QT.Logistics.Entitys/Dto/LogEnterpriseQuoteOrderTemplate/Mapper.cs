using Mapster;
using QT.Common.Extension;
using QT.Common.Security;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrderTemplate;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<LogEnterpriseQuoteOrderTemplateCrInput, LogEnterpriseQuoteOrderTemplateEntity>()
            .Map(dest=>dest.Property,src=>src.property.IsNotEmptyOrNull() ? src.property.ToJsonString():"");

        config.ForType<LogEnterpriseQuoteOrderTemplateEntity, LogEnterpriseQuoteOrderTemplateInfoOutput>()
            .Map(dest => dest.property, src => src.Property.IsNotEmptyOrNull() ? src.Property.ToObject<List<LogEnterpriseQuoteOrderTemplatePropertyInfo>>() : new List<LogEnterpriseQuoteOrderTemplatePropertyInfo>());

        config.ForType<LogEnterpriseQuoteOrderTemplateEntity, LogEnterpriseQuoteOrderTemplateListOutput>()
         .Map(dest => dest.property, src => src.Property.IsNotEmptyOrNull() ? src.Property.ToObject<List<LogEnterpriseQuoteOrderTemplatePropertyInfo>>() : new List<LogEnterpriseQuoteOrderTemplatePropertyInfo>());
        ;
    }
}