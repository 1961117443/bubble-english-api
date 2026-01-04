namespace QT.Logistics.Entitys.Dto.LogOrder;

/// <summary>
/// 订单管理输入参数.
/// </summary>
public class LogOrderListOutput
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

}