using QT.Common.Filter;

namespace QT.SDMS.Entitys.Dto.Marketer;

public class MarketerListQueryInput:PageInputBase
{
    /// <summary>
    /// 父级id
    /// </summary>
    public string pid { get; set; }

}
