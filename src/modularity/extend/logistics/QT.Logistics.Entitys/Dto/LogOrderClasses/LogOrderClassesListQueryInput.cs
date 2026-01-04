using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogOrderClasses;

/// <summary>
/// 订单装卸车记录列表查询输入
/// </summary>
public class LogOrderClassesListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

    /// <summary>
    /// 订单ID.
    /// </summary>
    public string orderId { get; set; }

    /// <summary>
    /// 车次ID.
    /// </summary>
    public string cId { get; set; }

    /// <summary>
    /// 装车人.
    /// </summary>
    public string inboundPerson { get; set; }

    /// <summary>
    /// 卸车人.
    /// </summary>
    public string outboundPerson { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string orderIdNo { get; set; }

    /// <summary>
    /// 车次编号.
    /// </summary>
    public string cIdNo { get; set; }

}