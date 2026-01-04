using NPOI.Util;
using System;

namespace QT.SDMS.Entitys.Dto.CustomerRefund;

public class CustomerRefundCrInput
{

    public string customerId { get; set; }

    /// <summary>
    /// 退款单号
    /// </summary>
    public string refundNo { get; set; }

    /// <summary>
    /// 来源（充值/账单等）
    /// </summary>
    public int? sourceType { get; set; }


    /// <summary>
    /// 来源表主键（充值Id 或 账单Id）
    /// </summary>
    public string sourceId { get; set; }

    /// <summary>
    /// 退款金额
    /// </summary>
    public decimal amount { get; set; }


    /// <summary>
    /// 退款原因
    /// </summary>
    public string reason { get; set; }


    /// <summary>
    /// 状态（处理中、成功、失败）
    /// </summary>
    public int? status { get; set; }

    /// <summary>
    /// 审核状态
    /// </summary>
    public int? auditStatus { get; set; }

    /// <summary>
    /// 原支付渠道
    /// </summary>
    public int? payChannel { get; set; }

    /// <summary>
    /// 退款完成时间
    /// </summary>
    public DateTime? refundTime { get; set; }     
}