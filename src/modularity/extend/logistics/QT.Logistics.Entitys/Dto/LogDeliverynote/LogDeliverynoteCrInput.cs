using QT.Logistics.Entitys.Dto.LogDeliverynoteOrder;

namespace QT.Logistics.Entitys.Dto.LogDeliverynote;

/// <summary>
/// 配送单修改输入参数.
/// </summary>
public class LogDeliverynoteCrInput
{
    /// <summary>
    /// 配送单号.
    /// </summary>
    public string orderNo { get; set; }

    /// <summary>
    /// 配送日期.
    /// </summary>
    public DateTime? orderDate { get; set; }

    /// <summary>
    /// 配送单订单明细.
    /// </summary>
    public List<LogDeliverynoteOrderCrInput> logDeliverynoteOrderList { get; set; }

    /// <summary>
    /// 收件点.
    /// </summary>
    public string pointId { get; set; }

}