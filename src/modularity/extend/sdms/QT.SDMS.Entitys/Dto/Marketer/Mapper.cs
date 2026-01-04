using Mapster;

namespace QT.SDMS.Entitys.Dto.Marketer;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        //config.NewConfig<MarketerCrInput, MarketerEntity>()
        //    .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);


        //config.NewConfig<MarketerUpInput, MarketerEntity>()
        //    .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);

        //config.NewConfig<MarketerEntity, MarketerOutput>()
        //    .Map(dest => dest.attachment, src => string.IsNullOrEmpty(src.Attachment) ? new List<FileControlsModel>() : src.Attachment.ToObject<List<FileControlsModel>>());
    }
}