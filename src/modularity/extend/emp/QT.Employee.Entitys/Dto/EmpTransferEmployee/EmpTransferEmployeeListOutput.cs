namespace QT.Emp.Entitys.Dto.EmpTransferEmployee;

/// <summary>
/// 调岗管理输入参数.
/// </summary>
public class EmpTransferEmployeeListOutput
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
    /// 员工id.
    /// </summary>
    public string employeeIdName { get; set; }

    /// <summary>
    /// 调入部门id.
    /// </summary>
    public string transferOrganizeId { get; set; }

    /// <summary>
    /// 调入岗位id.
    /// </summary>
    public string transferPositionId { get; set; }

    /// <summary>
    /// 调入部门id.
    /// </summary>
    public string transferOrganizeIdName { get; set; }

    /// <summary>
    /// 调入岗位id.
    /// </summary>
    public string transferPositionIdName { get; set; }

    /// <summary>
    /// 调岗原因.
    /// </summary>
    public string reason { get; set; }

    /// <summary>
    /// 调岗日期.
    /// </summary>
    public DateTime? transferTime { get; set; }

    /// <summary>
    /// 调入部门id.
    /// </summary>
    public string organizeIdName { get; set; }

    /// <summary>
    /// 调入岗位id.
    /// </summary>
    public string positionIdName { get; set; }

    /// <summary>
    /// 入职日期
    /// </summary>
    public DateTime? confrimJoinTime { get; set; }

}