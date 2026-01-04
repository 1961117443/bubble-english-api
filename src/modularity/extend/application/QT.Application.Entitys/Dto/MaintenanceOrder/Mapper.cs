using Mapster;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Security;
using QT.Iot.Application.Entity;

namespace QT.Iot.Application.Dto.MaintenanceOrder;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<MaintenanceOrderCrInput, MaintenanceOrderEntity>()
            .Map(dest => dest.AttachJson, src => src.attach.IsAny() ? src.attach.ToJsonString() : "");

        config.ForType<MaintenanceOrderEntity, MaintenanceOrderCrInput>()
            .Map(dest=>dest.attach,src=> src.AttachJson.IsNotEmptyOrNull()? src.AttachJson.ToObject<List<FileControlsModel>>(): new List<FileControlsModel>());


        config.ForType<MaintenanceOrderEntity, MaintenanceOrderOutput>()
            .Map(dest => dest.attach, src => src.AttachJson.IsNotEmptyOrNull() ? src.AttachJson.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>());
    }
}
