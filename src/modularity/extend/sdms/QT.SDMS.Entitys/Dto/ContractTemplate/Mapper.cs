using Mapster;
using QT.Common.Extension;
using QT.Common.Security;

namespace QT.SDMS.Entitys.Dto.ContractTemplate;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<ContractTemplateCrInput, ContractTemplateEntity>()
            .Map(dest=>dest.Property,src=>src.property.IsNotEmptyOrNull() ? src.property.ToJsonString():"");

        config.ForType<ContractTemplateEntity, ContractTemplateInfoOutput>()
            .Map(dest => dest.property, src => src.Property.IsNotEmptyOrNull() ? src.Property.ToObject<List<ContractTemplatePropertyInfo>>() : new List<ContractTemplatePropertyInfo>());

        config.ForType<ContractTemplateEntity, ContractTemplateListOutput>()
         .Map(dest => dest.property, src => src.Property.IsNotEmptyOrNull() ? src.Property.ToObject<List<ContractTemplatePropertyInfo>>() : new List<ContractTemplatePropertyInfo>());
        ;
    }
}