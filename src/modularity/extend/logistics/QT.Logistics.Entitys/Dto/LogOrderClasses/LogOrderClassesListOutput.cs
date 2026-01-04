namespace QT.Logistics.Entitys.Dto.LogOrderClasses;

/// <summary>
/// 订单装卸车记录输入参数.
/// </summary>
public class LogOrderClassesListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 订单ID.
    /// </summary>
    public string orderId { get; set; }

    /// <summary>
    /// 车次ID.
    /// </summary>
    public string cId { get; set; }

    /// <summary>
    /// 装车时间.
    /// </summary>
    public DateTime? inboundTime { get; set; }

    /// <summary>
    /// 装车人.
    /// </summary>
    public string inboundPerson { get; set; }

    /// <summary>
    /// 卸车时间.
    /// </summary>
    public DateTime? outboundTime { get; set; }

    /// <summary>
    /// 卸车人.
    /// </summary>
    public string outboundPerson { get; set; }

    /// <summary>
    /// 车次.
    /// </summary>
    public string cIdCode { get; set; }

    /// <summary>
    /// 订单编号
    /// </summary>
    public string orderIdOrderNo { get; set; }

    /// <summary>
    /// 装车人姓名.
    /// </summary>
    public string inboundPersonName { get; set; }

    /// <summary>
    /// 卸车人姓名.
    /// </summary>
    public string outboundPersonName { get; set; }

}