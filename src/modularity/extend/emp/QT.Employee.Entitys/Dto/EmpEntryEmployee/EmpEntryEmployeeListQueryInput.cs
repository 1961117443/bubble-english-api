using QT.Common.Filter;

namespace QT.Emp.Entitys.Dto.EmpEntryEmployee;

/// <summary>
/// 入职管理列表查询输入
/// </summary>
public class EmpEntryEmployeeListQueryInput : PageInputBase
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
    /// 姓名.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 手机号码.
    /// </summary>
    public string mobile { get; set; }

    /// <summary>
    /// 预计入职时间.
    /// </summary>
    public string confrimJoinTime { get; set; }

    /// <summary>
    /// 类型 1: 已入职的记录，0：入职待处理
    /// </summary>
    public int type { get; set; }
}