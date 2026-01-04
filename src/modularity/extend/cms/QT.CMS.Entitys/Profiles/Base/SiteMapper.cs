using Mapster;
using QT.CMS.Entitys.Dto.Article;
using QT.CMS.Entitys.Dto.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.CMS.Entitys.Profiles.Base;

public class SiteMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<SiteChannelField, SiteChannelFieldDto>()
            .Map(dest => dest.options, src => UtilHelper.GetCheckboxOrRadioOptions(src.ControlType, src.ItemOption))
            .Map(dest => dest.fieldValue, src => UtilHelper.GetCheckboxDefaultValue(src.ControlType, src.DefaultValue))
            ;
    }
}
