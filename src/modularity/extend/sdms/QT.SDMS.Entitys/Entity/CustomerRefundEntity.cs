using QT.Common.Contracts;
using SqlSugar;
using System.ComponentModel;

namespace QT.SDMS.Entitys.Entity;

/// <summary>
/// 客户退款记录表
/// </summary>
[SugarTable("sdms_customer_refund")]
public class CustomerRefundEntity : CUDEntityBase
{
    /// <summary>
    /// 客户ID
    /// </summary>
    [SugarColumn(ColumnName = "CustomerId")]
    public string CustomerId { get; set; }

    /// <summary>
    /// 退款单号（系统生成）
    /// </summary>
    [SugarColumn(ColumnName = "RefundNo")] public string RefundNo { get; set; }

    /// <summary>
    /// 来源（充值/账单等）
    /// </summary>
    [SugarColumn(ColumnName = "SourceType")] public SdmsRefundSourceType SourceType { get; set; }

    /// <summary>
    /// 来源表主键（充值Id 或 账单Id）
    /// </summary>
    [SugarColumn(ColumnName = "SourceId")] public string SourceId { get; set; }

    /// <summary>
    /// 退款金额
    /// </summary>
    [SugarColumn(ColumnName = "Amount")] public decimal Amount { get; set; }

    /// <summary>
    /// 退款原因
    /// </summary>
    [SugarColumn(ColumnName = "Reason")] public string Reason { get; set; }

    /// <summary>
    /// 状态（处理中、成功、失败）
    /// </summary>
    [SugarColumn(ColumnName = "Status")] public SdmsRefundStatus Status { get; set; }
    /// <summary>
    /// 审核状态
    /// </summary>
    [SugarColumn(ColumnName = "Status")] public int? AuditStatus { get; set; }

    /// <summary>
    /// 退款完成时间
    /// </summary>
    [SugarColumn(ColumnName = "RefundTime")] public DateTime? RefundTime { get; set; }

    /// <summary>
    /// 原支付渠道
    /// </summary>
    [SugarColumn(ColumnName = "PayChannel")] public SdmsPayChannel PayChannel { get; set; }
}

 
public enum SdmsRefundSourceType
{
    [Description("来自充值退款")] Recharge = 1, // 来自充值退款
    [Description("来自账单退款")] Bill = 2      // 来自账单退款
}

public enum SdmsRefundStatus
{
    [Description("退款中")] Processing = 1, // 退款中
    [Description("成功")] Success = 2,    // 成功
    [Description("失败")] Failed = 3      // 失败
}

public enum SdmsRefundAuditStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}
 