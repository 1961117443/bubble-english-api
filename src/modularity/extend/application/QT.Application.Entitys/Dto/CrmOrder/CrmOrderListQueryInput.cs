using QT.Common.Filter;

namespace QT.Iot.Application.Dto.CrmOrder;

public class CrmOrderListQueryInput:PageInputBase
{
    /// <summary>
    /// 订单日期范围
    /// </summary>
    public string orderDate { get; set; }
}
