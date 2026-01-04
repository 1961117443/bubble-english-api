using QT.Common.Filter;

namespace QT.Emp.Entitys.Dto.EmpTransferEmployee;

/// <summary>
/// 调岗管理列表查询输入
/// </summary>
public class EmpTransferEmployeeListQueryInput : PageInputBase
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
    /// 调岗日期.
    /// </summary>
    public string transferTime { get; set; }

    /// <summary>
    /// 类型 1: 已调岗的记录，0：调岗待处理
    /// </summary>
    public int type { get; set; }
}