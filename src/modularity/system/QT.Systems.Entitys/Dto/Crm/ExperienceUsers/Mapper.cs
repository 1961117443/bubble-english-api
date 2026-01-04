using Mapster;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Security;
using QT.Systems.Dto.Crm;
using QT.Systems.Entitys.Crm;

namespace QT.Systems.Entitys.Dto.Crm;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<CrmUserCommunicationCrInput, CrmUserCommunicationEntity>()
            .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);

        config.ForType<CrmUserCommunicationEntity, CrmUserCommunicationInfoOutput>()
            .Map(dest => dest.attachment, src => string.IsNullOrEmpty(src.Attachment) ? new List<FileControlsModel>() : src.Attachment.ToObject<List<FileControlsModel>>());

    }
}
