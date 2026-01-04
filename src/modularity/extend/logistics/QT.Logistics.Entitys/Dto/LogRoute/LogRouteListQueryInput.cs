using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogRoute;

/// <summary>
/// 物流线路列表查询输入
/// </summary>
public class LogRouteListQueryInput : PageInputBase
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
    /// 线路名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 线路编号.
    /// </summary>
    public string code { get; set; }

}