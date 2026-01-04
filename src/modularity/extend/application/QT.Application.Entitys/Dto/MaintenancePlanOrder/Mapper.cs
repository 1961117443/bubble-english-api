using Mapster;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Security;
using QT.Iot.Application.Entity;

namespace QT.Iot.Application.Dto.MaintenancePlanOrder;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<MaintenancePlanOrderCrInput, MaintenancePlanOrderEntity>()
            .Map(dest => dest.AttachJson, src => src.attach.IsAny() ? src.attach.ToJsonString() : "");

        config.ForType<MaintenancePlanOrderEntity, MaintenancePlanOrderCrInput>()
            .Map(dest=>dest.attach,src=> src.AttachJson.IsNotEmptyOrNull()? src.AttachJson.ToObject<List<FileControlsModel>>(): new List<FileControlsModel>());


        config.ForType<MaintenancePlanOrderEntity, MaintenancePlanOrderOutput>()
            .Map(dest => dest.attach, src => src.AttachJson.IsNotEmptyOrNull() ? src.AttachJson.ToObject<List<FileControlsModel>>() : new List<FileControlsModel>());
    }
}
