using QT.Logistics.Entitys.Dto.LogOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics.Entitys.Dto.LogOrderSettlement;

public class LogOrderSettlementListOutput : LogOrderListOutput
{
    /// <summary>
    /// 订单金额
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 分账人id
    /// </summary>
    public string accountUserId { get; set; }

    /// <summary>
    /// 分账人
    /// </summary>
    public string accountUserName { get; set; }

    /// <summary>
    /// 分账时间
    /// </summary>
    public DateTime? accountTime { get; set; }

    /// <summary>
    /// 分账状态
    /// </summary>
    public int accountStatus { get; set; }

    /// <summary>
    /// 平台分成
    /// </summary>
    public decimal platformAmount { get; set; }

    /// <summary>
    /// 收件点分成
    /// </summary>
    public decimal sendPointAmount { get; set; }

    /// <summary>
    /// 到达点分成
    /// </summary>
    public decimal reachPointAmount { get; set; }

    /// <summary>
    /// 结算人id
    /// </summary>
    public string settlementUserId { get; set; }

    /// <summary>
    /// 分账人
    /// </summary>
    public string settlementUserName { get; set; }

    /// <summary>
    /// 结算时间
    /// </summary>
    public DateTime? settlementTime { get; set; }

    /// <summary>
    /// 结算状态
    /// </summary>
    public int settlementStatus { get; set; }
}
