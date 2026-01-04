
namespace QT.Logistics.Entitys.Dto.LogOrderFinancial;

/// <summary>
/// 订单财务明细输出参数.
/// </summary>
public class LogOrderFinancialInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 运费.
    /// </summary>
    public decimal freight { get; set; }

    /// <summary>
    /// 支付方式.
    /// </summary>
    public string paymentMethod { get; set; }

    /// <summary>
    /// 收款账号.
    /// </summary>
    public string receivingAccount { get; set; }

    /// <summary>
    /// 收款时间.
    /// </summary>
    public DateTime? receivingTime { get; set; }

    /// <summary>
    /// 实际到款.
    /// </summary>
    public decimal actualPayment { get; set; }

    /// <summary>
    /// 手续费.
    /// </summary>
    public decimal handlingFee { get; set; }

    /// <summary>
    /// 支付账号.
    /// </summary>
    public string paymentAccount { get; set; }

    /// <summary>
    /// 结算状态.
    /// </summary>
    public string settlementStatus { get; set; }

    /// <summary>
    /// 结算时间.
    /// </summary>
    public DateTime? settlementTime { get; set; }

}