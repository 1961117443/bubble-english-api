namespace QT.Emp.Entitys.Dto.EmpInterviewEmployee;

/// <summary>
/// 面试管理更新输入.
/// </summary>
public class EmpInterviewEmployeeUpInput //: EmpInterviewEmployeeCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 预计面试时间.
    /// </summary>
    public DateTime? interviewTime { get; set; }
}


public class EmpInterviewEmployeeEntryInput : EmpInterviewEmployeeCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }


    /// <summary>
    /// 计划入职日期.
    /// </summary>
    public DateTime? confrimJoinTime { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 面试结果
    /// </summary>
    public string result { get; set; }
}