
namespace QT.Emp.Entitys.Dto.EmpEmployeeFamily;

/// <summary>
/// 员工家庭信息输出参数.
/// </summary>
public class EmpEmployeeFamilyInfoOutput
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
    /// 姓名.
    /// </summary>
    public string memberName { get; set; }

    /// <summary>
    /// 关系.
    /// </summary>
    public string memberRelation { get; set; }

    /// <summary>
    /// 性别.
    /// </summary>
    public string memberGender { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    public string memberPhone { get; set; }

    /// <summary>
    /// 生日.
    /// </summary>
    public DateTime? memberBirthday { get; set; }

}