using QT.VisualDev.Entitys.Dto.VisualDev;
using Mapster;

namespace QT.VisualDev.Entitys.Mapper;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<VisualDevEntity, VisualDevSelectorOutput>()
            .Map(dest => dest.parentId, src => src.Category);
    }
}
