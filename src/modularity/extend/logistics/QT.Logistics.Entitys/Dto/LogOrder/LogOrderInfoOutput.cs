using QT.Common.Models;
using QT.Logistics.Entitys.Dto.LogOrderAttachment;
using QT.Logistics.Entitys.Dto.LogOrderDetail;
using QT.Logistics.Entitys.Dto.LogOrderFinancial;
using SqlSugar;

namespace QT.Logistics.Entitys.Dto.LogOrder;

/// <summary>
/// 订单管理输出参数.
/// </summary>
public class LogOrderInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string orderNo { get; set; }

    /// <summary>
    /// 寄件配送点id.
    /// </summary>
    public string sendPointId { get; set; }

    /// <summary>
    /// 送达配送点id.
    /// </summary>
    public string reachPointId { get; set; }

    /// <summary>
    /// 订单日期.
    /// </summary>
    public DateTime? orderDate { get; set; }

    /// <summary>
    /// 订单状态.
    /// </summary>
    public string orderStatus { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 寄件人姓名.
    /// </summary>
    public string shipperName { get; set; }

    /// <summary>
    /// 寄件人电话.
    /// </summary>
    public string shipperPhone { get; set; }

    /// <summary>
    /// 寄件地址.
    /// </summary>
    public string shipperAddress { get; set; }

    /// <summary>
    /// 收件人姓名.
    /// </summary>
    public string recipientName { get; set; }

    /// <summary>
    /// 收件人电话.
    /// </summary>
    public string recipientPhone { get; set; }

    /// <summary>
    /// 收件地址.
    /// </summary>
    public string recipientAddress { get; set; }

    /// <summary>
    /// 附件路径.
    /// </summary>
    public List<LogOrderAttachmentInfoOutput> logOrderAttachmentList { get; set; }

    /// <summary>
    /// 订单物品明细.
    /// </summary>
    public List<LogOrderDetailInfoOutput> logOrderDetailList { get; set; }

    /// <summary>
    /// 订单财务明细.
    /// </summary>
    public List<LogOrderFinancialInfoOutput> logOrderFinancialList { get; set; }

    /// <summary>
    /// 分账人id.
    /// </summary>
    public string accountUserId { get; set; }

    /// <summary>
    /// 分账时间.
    /// </summary>
    public DateTime? accountTime { get; set; }


    /// <summary>
    /// 平台分成.
    /// </summary>
    public decimal platformAmount { get; set; }

    /// <summary>
    /// 收件点分成.
    /// </summary>
    public decimal sendPointAmount { get; set; }

    /// <summary>
    /// 到达点分成.
    /// </summary>
    public decimal reachPointAmount { get; set; }

    /// <summary>
    /// 结算人id.
    /// </summary>
    public string settlementUserId { get; set; }

    /// <summary>
    /// 结算时间.
    /// </summary>
    public DateTime? settlementTime { get; set; }


    /// <summary>
    /// 订单收款明细.
    /// </summary>
    public List<LogOrderCollectionInfoOutput> logOrderCollectionList { get; set; }
}