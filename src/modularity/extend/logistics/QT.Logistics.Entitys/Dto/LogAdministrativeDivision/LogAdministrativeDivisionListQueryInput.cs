using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogAdministrativeDivision;

/// <summary>
/// 区域管理列表查询输入
/// </summary>
public class LogAdministrativeDivisionListQueryInput : PageInputBase
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
    /// 区域名称.
    /// </summary>
    public string name { get; set; }

}