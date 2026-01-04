
namespace QT.Emp.Entitys.Dto.EmpEmployeeUrgent;

/// <summary>
/// 员工紧急联系人输出参数.
/// </summary>
public class EmpEmployeeUrgentInfoOutput
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
    /// 紧急联系人姓名.
    /// </summary>
    public string urgentContactsName { get; set; }

    /// <summary>
    /// 联系人关系.
    /// </summary>
    public string urgentContactsRelation { get; set; }

    /// <summary>
    /// 联系人电话.
    /// </summary>
    public string urgentContactsPhone { get; set; }

}