using Mapster;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Security;
using QT.Iot.Application.Entity;

namespace QT.Iot.Application.Dto.CrmProject;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        //config.NewConfig<CrmProjectCrInput, CrmProjectEntity>()
        //    .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);


        //config.NewConfig<CrmProjectUpInput, CrmProjectEntity>()
        //    .Map(dest => dest.Attachment, src => src.attachment.IsAny() ? src.attachment.ToJsonString() : string.Empty);

        //config.NewConfig<CrmProjectEntity, CrmProjectOutput>()
        //    .Map(dest => dest.attachment, src => string.IsNullOrEmpty(src.Attachment) ? new List<FileControlsModel>() : src.Attachment.ToObject<List<FileControlsModel>>());
    }
}