
namespace QT.Emp.Entitys.Dto.EmpEmployeeEdu;

/// <summary>
/// 员工学历输出参数.
/// </summary>
public class EmpEmployeeEduInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 员工ID.
    /// </summary>
    public string employeeId { get; set; }

    /// <summary>
    /// 学历.
    /// </summary>
    public string highestEdu { get; set; }

    /// <summary>
    /// 毕业院校.
    /// </summary>
    public string graduateSchool { get; set; }

    /// <summary>
    /// 毕业时间.
    /// </summary>
    public DateTime? graduationTime { get; set; }

    /// <summary>
    /// 所学专业.
    /// </summary>
    public string major { get; set; }

}