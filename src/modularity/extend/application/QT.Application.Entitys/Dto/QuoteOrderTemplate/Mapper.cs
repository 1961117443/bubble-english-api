using Mapster;
using QT.Common.Extension;
using QT.Common.Security;

namespace QT.Extend.Entitys.Dto.QuoteOrderTemplate;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<QuoteOrderTemplateCrInput, QuoteOrderTemplateEntity>()
            .Map(dest=>dest.Property,src=>src.property.IsNotEmptyOrNull() ? src.property.ToJsonString():"");

        config.ForType<QuoteOrderTemplateEntity, QuoteOrderTemplateInfoOutput>()
            .Map(dest => dest.property, src => src.Property.IsNotEmptyOrNull() ? src.Property.ToObject<List<QuoteOrderTemplatePropertyInfo>>() : new List<QuoteOrderTemplatePropertyInfo>());

        config.ForType<QuoteOrderTemplateEntity, QuoteOrderTemplateListOutput>()
         .Map(dest => dest.property, src => src.Property.IsNotEmptyOrNull() ? src.Property.ToObject<List<QuoteOrderTemplatePropertyInfo>>() : new List<QuoteOrderTemplatePropertyInfo>());
        ;
    }
}