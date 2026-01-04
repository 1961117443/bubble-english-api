using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogOrderDelivery;

/// <summary>
/// 出入库记录列表查询输入
/// </summary>
public class LogOrderDeliveryListQueryInput : PageInputBase
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
    /// 订单.
    /// </summary>
    public string orderIdOrderNo { get; set; }

    /// <summary>
    /// 配送点ID.
    /// </summary>
    public string pointId { get; set; }

}