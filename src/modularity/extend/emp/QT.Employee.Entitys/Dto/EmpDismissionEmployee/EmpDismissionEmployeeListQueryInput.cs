using QT.Common.Filter;

namespace QT.Emp.Entitys.Dto.EmpDismissionEmployee;

/// <summary>
/// 离职管理列表查询输入
/// </summary>
public class EmpDismissionEmployeeListQueryInput : PageInputBase
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
    /// 离职日期.
    /// </summary>
    public string lastWorkDay { get; set; }

    /// <summary>
    /// 类型 1: 已离职的记录，0：离职待处理
    /// </summary>
    public int type { get; set; }

}