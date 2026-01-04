using Mapster;
using QT.CMS.Entitys.Dto.Base;

namespace QT.CMS.Entitys.Profiles.Base;

public class AreaProfile : IRegister
{
    /// <summary>
    /// 省市区实体映射
    /// </summary>
    public void Register(TypeAdapterConfig config)
    {
        //将源数据映射到DTO
        config.ForType<Areas, AreasDto>();
        config.ForType<Areas, AreasEditDto>();
        //将DTO映射到源数据
        config.ForType<AreasEditDto, Areas>();
    }
}
