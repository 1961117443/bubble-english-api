using Mapster;

namespace QT.SDMS.Entitys.Dto.Customer;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        //config.NewConfig<CrmCustomerCrInput, CrmCustomerEntity>()
        //    .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);


        //config.NewConfig<CrmCustomerUpInput, CrmCustomerEntity>()
        //    .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);

        //config.NewConfig<CrmCustomerEntity, CrmCustomerOutput>()
        //    .Map(dest => dest.attachment, src => string.IsNullOrEmpty(src.Attachment) ? new List<FileControlsModel>() : src.Attachment.ToObject<List<FileControlsModel>>());
    }
}