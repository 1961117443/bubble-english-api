namespace QT.Emp.Entitys.Dto.EmpEntryEmployee;

/// <summary>
/// 入职管理更新输入.
/// </summary>
public class EmpEntryEmployeeUpInput //: EmpEntryEmployeeCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 预计入职时间.
    /// </summary>
    public DateTime? confrimJoinTime { get; set; }
}


public class EmpEntryEmployeeEntryInput : EmpEntryEmployeeCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }


    /// <summary>
    /// 员工类型.
    /// </summary>
    public string employeeType { get; set; }

    /// <summary>
    /// 员工状态.
    /// </summary>
    public string employeeStatus { get; set; }

    /// <summary>
    /// 试用期.
    /// </summary>
    public string probationPeriodType { get; set; }


    /// <summary>
    /// 计划转正日期.
    /// </summary>
    public DateTime? planRegularTime { get; set; }

    /// <summary>
    /// 工号.
    /// </summary>
    public string jobNumber { get; set; }
}

/// <summary>
/// 转正管理修改输入参数.
/// </summary>
public class EmpProbationEmployeeUpInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 转正时间.
    /// </summary>
    public DateTime? regularTime { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string remark { get; set; }

}