
namespace QT.Emp.Entitys.Dto.EmpEmployeeFamily;

/// <summary>
/// 员工家庭信息修改输入参数.
/// </summary>
public class EmpEmployeeFamilyCrInput
{
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