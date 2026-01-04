namespace QT.Emp.Entitys.Dto.EmpEntryEmployee;

/// <summary>
/// 入职管理输入参数.
/// </summary>
public class EmpEntryEmployeeListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 姓名.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 手机号码.
    /// </summary>
    public string mobile { get; set; }

    /// <summary>
    /// 部门id.
    /// </summary>
    public string organizeId { get; set; }

    /// <summary>
    /// 岗位id.
    /// </summary>
    public string positionId { get; set; }

    /// <summary>
    /// 部门.
    /// </summary>
    public string organizeIdName { get; set; }

    /// <summary>
    /// 岗位.
    /// </summary>
    public string positionIdName { get; set; }

    /// <summary>
    /// 预计入职时间.
    /// </summary>
    public DateTime? confrimJoinTime { get; set; }

    /// <summary>
    /// 剩余天数
    /// </summary>
    public int remainDays { get; set; }

    /// <summary>
    /// 员工类型.
    /// </summary>
    public string employeeType { get; set; }

    /// <summary>
    /// 员工状态.
    /// </summary>
    public string employeeStatus { get; set; }
}