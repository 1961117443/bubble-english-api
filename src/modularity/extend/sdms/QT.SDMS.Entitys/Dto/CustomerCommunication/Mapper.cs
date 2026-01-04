using Mapster;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Security;
using QT.Systems.Dto.Crm;
using QT.Systems.Entitys.Crm;

namespace QT.SDMS.Entitys.Dto.CustomerCommunication;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<CustomerCommunicationCrInput, CustomerCommunicationEntity>()
            .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);

        config.ForType<CustomerCommunicationEntity, CustomerCommunicationInfoOutput>()
            .Map(dest => dest.attachment, src => string.IsNullOrEmpty(src.Attachment) ? new List<FileControlsModel>() : src.Attachment.ToObject<List<FileControlsModel>>());

    }
}
