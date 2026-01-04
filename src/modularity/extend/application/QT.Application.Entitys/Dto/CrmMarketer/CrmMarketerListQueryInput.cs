using QT.Common.Filter;

namespace QT.Iot.Application.Dto.CrmMarketer;

public class CrmMarketerListQueryInput:PageInputBase
{
    /// <summary>
    /// 父级id
    /// </summary>
    public string pid { get; set; }

}
