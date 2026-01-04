using QT.SDMS.Entitys.Dto.OrderCommission;

namespace QT.SDMS.Entitys.Dto.Order;

public class OrderOutput: OrderUpInput
{
    /// <summary>
    /// 佣金明细
    /// </summary>
    public List<OrderCommissionOutput> orderCommissions { get; set; }
}