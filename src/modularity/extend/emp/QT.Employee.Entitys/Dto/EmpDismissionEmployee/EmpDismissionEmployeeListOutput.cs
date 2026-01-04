namespace QT.Emp.Entitys.Dto.EmpDismissionEmployee;

/// <summary>
/// 离职管理输入参数.
/// </summary>
public class EmpDismissionEmployeeListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 员工id.
    /// </summary>
    public string employeeId { get; set; }

    /// <summary>
    /// 员工.
    /// </summary>
    public string employeeIdName { get; set; }

    /// <summary>
    /// 离职日期.
    /// </summary>
    public DateTime? lastWorkDay { get; set; }

    /// <summary>
    /// 离职原因.
    /// </summary>
    public string reason { get; set; }


    /// <summary>
    /// 部门.
    /// </summary>
    public string organizeIdName { get; set; }

    /// <summary>
    /// 岗位.
    /// </summary>
    public string positionIdName { get; set; }

    /// <summary>
    /// 入职日期.
    /// </summary>
    public DateTime? confrimJoinTime { get; set; }

}