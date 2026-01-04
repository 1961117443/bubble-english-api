namespace QT.Emp.Entitys.Dto.EmpEmployee;

/// <summary>
/// 员工档案输入参数.
/// </summary>
public class EmpEmployeeListOutput
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
    /// 部门id.
    /// </summary>
    public string organizeId { get; set; }

    /// <summary>
    /// 部门.
    /// </summary>
    public string organizeIdName { get; set; }

    /// <summary>
    /// 岗位id.
    /// </summary>
    public string positionId { get; set; }

    /// <summary>
    /// 岗位.
    /// </summary>
    public string positionIdName { get; set; }

    /// <summary>
    /// 手机号码.
    /// </summary>
    public string mobile { get; set; }

    /// <summary>
    /// 入职时间.
    /// </summary>
    public DateTime? confrimJoinTime { get; set; }

    /// <summary>
    /// 员工类型.
    /// </summary>
    public string employeeType { get; set; }


    /// <summary>
    /// 生日时间.
    /// </summary>
    public DateTime? birthTime { get; set; }


    /// <summary>
    /// 计划转正日期.
    /// </summary>
    public DateTime? planRegularTime { get; set; }


    /// <summary>
    /// 试用期.
    /// </summary>
    public string probationPeriodType { get; set; }
}