
namespace QT.Logistics.Entitys.Dto.LogOrderClasses;

/// <summary>
/// 订单装卸车记录输出参数.
/// </summary>
public class LogOrderClassesInfoOutput
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
    /// 订单编号
    /// </summary>
    public string orderIdOrderNo { get; set; }

    /// <summary>
    /// 状态（0：待装车，1：已装车，2：已卸车）.
    /// </summary>
    public int status { get; set; }

}