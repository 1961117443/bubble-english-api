using QT.Common.Contracts;
using SqlSugar;

namespace QT.Iot.Application.Entity;

/// <summary>
/// 销售订单提成
/// </summary>
[SugarTable("crm_order_commission")]
public class CrmOrderCommissionEntity : CUDEntityBase
{
    /// <summary>
    /// 单号
    /// </summary>
    public string No { get; set; }

    /// <summary>
    /// 订单id
    /// </summary>
    public string FId { get; set; }

    /// <summary>
    /// 业务员id
    /// </summary>
    public string UserId { get; set; }


    /// <summary>
    /// 提成占比
    /// </summary>
    public decimal Proportion { get; set; }


    /// <summary>
    /// 提成金额
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 结算人
    /// </summary>
    public string BalanceUserId { get; set; }


    /// <summary>
    /// 结算时间
    /// </summary>
    public DateTime? BalanceTime { get; set; }
}