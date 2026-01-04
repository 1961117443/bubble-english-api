using QT.Application.Entitys.Enum.FreshDelivery;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderCw;

/// <summary>
/// 订单信息输入参数.
/// </summary>
public class ErpOrderCwListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 公司ID.
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 代下单人员.
    /// </summary>
    public string createUid { get; set; }

    /// <summary>
    /// 代下单时间.
    /// </summary>
    public DateTime? createTime { get; set; }

    /// <summary>
    /// 客户ID.
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 约定送货时间.
    /// </summary>
    public DateTime? posttime { get; set; }


    /// <summary>
    /// 客户.
    /// </summary>
    public string cidName { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 订单状态
    /// </summary>
    public OrderStateEnum state { get; set; }

    /// <summary>
    /// 收获确认时间
    /// </summary>
    public DateTime? receiveTime { get; set; }

    /// <summary>
    /// 对账单开具日期
    /// </summary>
    public DateTime? billDate { get; set; }

    /// <summary>
    /// 发票开具日期
    /// </summary>
    public DateTime? invoiceDate { get; set; }

    /// <summary>
    /// 收款日期
    /// </summary>
    public DateTime? receiptsDate { get; set; }

    /// <summary>
    /// 星期几
    /// </summary>
    public string dayOfWeek { get; set; }
}