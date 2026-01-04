
using QT.Logistics.Entitys.Dto.LogOrder;

namespace QT.Logistics.Entitys.Dto.LogDeliverynoteOrder;

/// <summary>
/// 配送单订单明细输出参数.
/// </summary>
public class LogDeliverynoteOrderInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 订单ID.
    /// </summary>
    public string orderId { get; set; }

    public LogOrderListOutput logOrder { get; set; }

}