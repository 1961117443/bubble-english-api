using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogClasses;

/// <summary>
/// 班次信息列表查询输入
/// </summary>
public class LogClassesListQueryInput : PageInputBase
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
    /// 班次名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 发车时间.
    /// </summary>
    public string departureTime { get; set; }

    /// <summary>
    /// 线路ID.
    /// </summary>
    public string routeId { get; set; }

}