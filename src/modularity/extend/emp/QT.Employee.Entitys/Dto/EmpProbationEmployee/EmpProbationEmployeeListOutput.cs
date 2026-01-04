namespace QT.Emp.Entitys.Dto.EmpProbationEmployee;

/// <summary>
/// 转正管理输入参数.
/// </summary>
public class EmpProbationEmployeeListOutput
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
    /// 转正日期.
    /// </summary>
    public DateTime? regularTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }


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