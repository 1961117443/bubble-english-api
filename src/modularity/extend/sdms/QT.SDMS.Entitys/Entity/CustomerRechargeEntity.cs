using QT.Common.Contracts;
using SqlSugar;
using System.ComponentModel;

namespace QT.SDMS.Entitys.Entity;

/// <summary>
/// 客户充值记录表
/// </summary>
[SugarTable("sdms_customer_recharge")]
public class CustomerRechargeEntity : CUDEntityBase
{
    /// <summary>
    /// 客户ID
    /// </summary>
    [SugarColumn(ColumnName = "CustomerId")]
    public string CustomerId { get; set; }

    /// <summary>
    /// 充值单号
    /// </summary>
    [SugarColumn(ColumnName = "RechargeNo")] public string RechargeNo { get; set; }

    /// <summary>
    /// 充值金额
    /// </summary>
    [SugarColumn(ColumnName = "Amount")] public decimal Amount { get; set; }

    /// <summary>
    /// 支付渠道
    /// </summary>
    [SugarColumn(ColumnName = "PayChannel")] public SdmsPayChannel PayChannel { get; set; }

    /// <summary>
    /// 支付成功时间（成功后填）
    /// </summary>
    [SugarColumn(ColumnName = "PayTime")] public DateTime? PayTime { get; set; }

    /// <summary>
    /// 充值状态
    /// </summary>
    [SugarColumn(ColumnName = "Status")] public SdmsRechargeStatus Status { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark")] public string Remark { get; set; }
}


public enum SdmsPayChannel:int
{
   [Description("微信")] WeChat = 1,      // 微信
   [Description("支付宝")] Alipay = 2,      // 支付宝
   [Description("银行转账")] BankTransfer = 3,// 银行转账
   [Description("现金")] Cash = 4,        // 现金
    [Description("其他")] Other = 9
}

public enum SdmsRechargeStatus : int
{
   [Description("待支付")] Pending = 1,   // 待支付
   [Description("成功")] Success = 2,   // 成功
   [Description("失败")] Failed = 3,    // 失败
    [Description("已取消")] Canceled = 4   // 已取消
}
