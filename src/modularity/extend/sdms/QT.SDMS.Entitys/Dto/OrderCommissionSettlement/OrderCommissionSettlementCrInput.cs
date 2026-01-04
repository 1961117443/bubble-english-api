using NPOI.Util;
using QT.SDMS.Entitys.Dto.OrderCommission;
using System;

namespace QT.SDMS.Entitys.Dto.OrderCommissionSettlement;

public class OrderCommissionSettlementCrInput
{
    /// <summary>
    /// 业务员id
    /// </summary>
    public string userId { get; set; }

    /// <summary>
    /// 结算单号
    /// </summary>
    public string settlementNo { get; set; }


    /// <summary>
    /// 本次结算总金额
    /// </summary>
    public decimal totalCommission { get; set; }


    /// <summary>
    /// 状态：待付款/已付款/已取消
    /// </summary>
    public int? status { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? remark { get; set; }

    /// <summary>
    /// 付款时间
    /// </summary>
    public DateTime? payTime { get; set; }

    /// <summary>
    /// 佣金单集合
    /// </summary>
    public List<OrderCommissionListOutput> commissionList { get; set; }
}