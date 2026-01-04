
namespace QT.Emp.Entitys.Dto.EmpDismissionEmployee;

/// <summary>
/// 离职管理输出参数.
/// </summary>
public class EmpDismissionEmployeeInfoOutput
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

}