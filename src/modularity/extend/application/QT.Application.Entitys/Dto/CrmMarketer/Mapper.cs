using Mapster;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Security;
using QT.Iot.Application.Entity;

namespace QT.Iot.Application.Dto.CrmMarketer;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CrmMarketerCrInput, CrmMarketerEntity>()
            .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);


        config.NewConfig<CrmMarketerUpInput, CrmMarketerEntity>()
            .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);

        config.NewConfig<CrmMarketerEntity, CrmMarketerOutput>()
            .Map(dest => dest.attachment, src => string.IsNullOrEmpty(src.Attachment) ? new List<FileControlsModel>() : src.Attachment.ToObject<List<FileControlsModel>>());
    }
}