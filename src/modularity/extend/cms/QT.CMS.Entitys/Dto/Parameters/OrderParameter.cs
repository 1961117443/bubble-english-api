namespace QT.CMS.Entitys.Dto.Parameter;

/// <summary>
/// 订单查询参数
/// </summary>
public class OrderParameter : BaseParameter
{
    /// <summary>
    /// 订单类型
    /// </summary>
    public int OrderType { get; set; } = -1;

    /// <summary>
    /// 支付方式
    /// </summary>
    public int PaymentId { get; set; }

    /// <summary>
    /// 支付状态
    /// </summary>
    public int PaymentStatus { get; set; } = -1;

    /// <summary>
    /// 配送方式
    /// </summary>
    public int DeliveryId { get; set; }

    /// <summary>
    /// 配送状态
    /// </summary>
    public int DeliveryStatus { get; set; } = -1;

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}
