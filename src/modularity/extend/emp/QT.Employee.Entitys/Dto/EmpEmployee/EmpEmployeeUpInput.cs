using QT.Emp.Entitys.Dto.EmpEmployeeEdu;
using QT.Emp.Entitys.Dto.EmpEmployeeFamily;
using QT.Emp.Entitys.Dto.EmpEmployeeUrgent;
namespace QT.Emp.Entitys.Dto.EmpEmployee;

/// <summary>
/// 员工档案更新输入.
/// </summary>
public class EmpEmployeeUpInput : EmpEmployeeCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }
    /// <summary>
    /// 员工学历.
    /// </summary>
    public new List<EmpEmployeeEduUpInput> empEmployeeEduList { get; set; }

    /// <summary>
    /// 员工家庭信息.
    /// </summary>
    public new List<EmpEmployeeFamilyUpInput> empEmployeeFamilyList { get; set; }

    /// <summary>
    /// 员工紧急联系人.
    /// </summary>
    public new List<EmpEmployeeUrgentUpInput> empEmployeeUrgentList { get; set; }

}