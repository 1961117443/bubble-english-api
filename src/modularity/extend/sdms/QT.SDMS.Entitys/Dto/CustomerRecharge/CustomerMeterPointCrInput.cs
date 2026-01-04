namespace QT.SDMS.Entitys.Dto.CustomerRecharge;

public class CustomerRechargeCrInput
{

    public string customerId { get; set; }

    /// <summary>
    /// 充值单号
    /// </summary>
    public string rechargeNo { get; set; }

    /// <summary>
    /// 充值金额
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 支付渠道
    /// </summary>
    public int? payChannel { get; set; }

    /// <summary>
    /// 支付成功时间（成功后填）
    /// </summary>
    public DateTime? payTime { get; set; }

    /// <summary>
    /// 充值状态
    /// </summary>
    public int? status { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string remark { get; set; }
}