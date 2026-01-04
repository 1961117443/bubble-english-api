using QT.Common.Filter;

namespace QT.Emp.Entitys.Dto.BaseModuleremind;

/// <summary>
/// 模块提醒列表查询输入
/// </summary>
public class BaseModuleremindListQueryInput : PageInputBase
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
    public string fullName { get; set; }

}