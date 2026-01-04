using QT.Common.Filter;

namespace QT.Emp.Entitys.Dto.EmpEmployee;

/// <summary>
/// 员工档案列表查询输入
/// </summary>
public class EmpEmployeeListQueryInput : PageInputBase
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
    /// 部门id.
    /// </summary>
    public string organizeId { get; set; }

    /// <summary>
    /// 入职时间.
    /// </summary>
    public string confrimJoinTime { get; set; }


    /// <summary>
    /// 生日月份
    /// </summary>
    public string birthMonth { get; set; }

    /// <summary>
    /// 入职月份.
    /// </summary>
    public string confrimJoinMonth { get; set; }

    /// <summary>
    /// 员工状态
    /// </summary>
    public string employeeStatus { get; set; }

    /// <summary>
    /// 计划转正时间
    /// </summary>
    public string planRegularTime { get; set; }
}