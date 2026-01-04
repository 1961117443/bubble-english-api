using QT.Common.Contracts;
using QT.Common.Filter;
using SqlSugar;
using System.ComponentModel;

namespace QT.SDMS.Entitys.Entity;

/// <summary>
/// 销售订单提成
/// </summary>
[SugarTable("sdms_order_commission")]
public class OrderCommissionEntity : CUDEntityBase
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
            
    /// <summary>
    /// 结算单id
    /// </summary>
    public string SettleId { get; set; }


    /// <summary>
    /// 数据来源id
    /// </summary>
    public string SourceId { get; set; }

    /// <summary>
    /// 来源（充值、账单）
    /// </summary>
    public SdmsCommissionSourceType? SourceType { get; set; }

    /// <summary>
    /// 佣金状态（待结算/已结算/拒绝）
    /// </summary>
    public SdmsCommissionStatus Status { get; set; }

    /// <summary>
    /// 原始金额（账单/充值金额）
    /// </summary>
    public decimal? OrderAmount { get; set; }
}


public enum SdmsCommissionSourceType : int
{
    [Description("充值佣金")] Recharge = 1,   // 充值佣金
    [Description("电费账单佣金")] Bill = 2   // 电费账单佣金
}

public enum SdmsCommissionStatus
{
    [Description("待结算")][TagStyle("warning")] Pending = 1, // 待结算
    [Description("已结算")][TagStyle("success")] Settled = 2, // 已结算
    [Description("驳回/取消")][TagStyle("danger")] Rejected = 3 // 驳回/取消
}
