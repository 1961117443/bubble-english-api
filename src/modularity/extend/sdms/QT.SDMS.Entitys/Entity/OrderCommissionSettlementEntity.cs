using QT.Common.Contracts;
using SqlSugar;
using System.ComponentModel;

namespace QT.SDMS.Entitys.Entity;

/// <summary>
/// 售电系统-佣金结算单
/// </summary>
[SugarTable("sdms_order_commission_settlement")]
public class OrderCommissionSettlementEntity : CUDEntityBase
{
    /// <summary>
    /// 结算单号
    /// </summary>
    [SugarColumn(ColumnName = "SettlementNo")] public string SettlementNo { get; set; }

    /// <summary>
    /// 销售人员
    /// </summary>
    [SugarColumn(ColumnName = "UserId")] public string UserId { get; set; }

    /// <summary>
    /// 本次结算总金额
    /// </summary>
    [SugarColumn(ColumnName = "TotalCommission")] public decimal TotalCommission { get; set; }

    /// <summary>
    /// 状态：待付款/已付款/已取消
    /// </summary>
    [SugarColumn(ColumnName = "Status")] public SdmsSettlementStatus Status { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark")] public string Remark { get; set; }

    /// <summary>
    /// 付款时间
    /// </summary>
    [SugarColumn(ColumnName = "PayTime")] public DateTime? PayTime { get; set; }
}

 
 
public enum SdmsSettlementStatus : int
{
    [Description("待付款")] Pending = 1,  // 待付款
    [Description("已付款")] Paid = 2,     // 已付款
    [Description("已取消")] Canceled = 3  // 已取消
}
