using QT.Common.Filter;

namespace QT.SDMS.Entitys.Dto.Order;

public class OrderListQueryInput:PageInputBase
{
    /// <summary>
    /// 订单日期范围
    /// </summary>
    public string orderDate { get; set; }
}
