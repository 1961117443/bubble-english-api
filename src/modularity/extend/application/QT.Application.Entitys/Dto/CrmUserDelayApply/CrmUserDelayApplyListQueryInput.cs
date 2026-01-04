using QT.Common.Filter;

namespace QT.Iot.Application.Dto.CrmUserDelayApply;

public class CrmUserDelayApplyListQueryInput:PageInputBase
{
    /// <summary>
    /// 状态
    /// </summary>
    public int? status { get; set; }
}
