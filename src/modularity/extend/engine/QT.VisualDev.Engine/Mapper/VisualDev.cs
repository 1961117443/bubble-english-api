using QT.Common.Models.VisualDev;
using Mapster;

namespace QT.VisualDev.Engine.Mapper;

internal class VisualDev : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<FieldsModel, ListSearchParametersModel>()
           .Map(dest => dest.qtKey, src => src.__config__.qtKey)
           .Map(dest => dest.format, src => src.format)
           .Map(dest => dest.multiple, src => src.multiple)
           .Map(dest => dest.searchType, src => src.searchType)
           .Map(dest => dest.vModel, src => src.__vModel__);
    }
}