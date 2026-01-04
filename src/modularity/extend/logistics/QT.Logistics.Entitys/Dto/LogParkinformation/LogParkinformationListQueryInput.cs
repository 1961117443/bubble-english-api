using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogParkinformation;

/// <summary>
/// 物流园信息列表查询输入
/// </summary>
public class LogParkinformationListQueryInput : PageInputBase
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
    /// 物流园名称.
    /// </summary>
    public string name { get; set; }

}