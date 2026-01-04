
namespace QT.Emp.Entitys.Dto.EmpDismissionEmployee;

/// <summary>
/// 离职管理修改输入参数.
/// </summary>
public class EmpDismissionEmployeeCrInput
{
    /// <summary>
    /// 员工id.
    /// </summary>
    public string employeeId { get; set; }

    /// <summary>
    /// 离职日期.
    /// </summary>
    public DateTime? lastWorkDay { get; set; }

    /// <summary>
    /// 离职原因.
    /// </summary>
    public List<string> reason { get; set; }

    /// <summary>
    /// 离职备注.
    /// </summary>
    public string remark { get; set; }


    public string organizeId { get; set; }


    public string positionId { get; set; }

    /// <summary>
    /// 入职日期.
    /// </summary>
    public DateTime? confrimJoinTime { get; set; }
}