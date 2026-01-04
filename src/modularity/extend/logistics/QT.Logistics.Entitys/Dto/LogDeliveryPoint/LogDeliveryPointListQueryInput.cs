using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogDeliveryPoint;

/// <summary>
/// 配送点管理列表查询输入
/// </summary>
public class LogDeliveryPointListQueryInput : PageInputBase
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
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    public string code { get; set; }

}