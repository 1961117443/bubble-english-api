using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Emp.Entitys;

/// <summary>
/// 员工紧急联系人实体.
/// </summary>
[SugarTable("emp_employee_urgent")]
[Tenant(ClaimConst.TENANTID)]
public class EmpEmployeeUrgentEntity :CUDEntityBase
{
    /// <summary>
    /// 员工ID.
    /// </summary>
    [SugarColumn(ColumnName = "EmployeeId")]
    public string EmployeeId { get; set; }

    /// <summary>
    /// 紧急联系人姓名.
    /// </summary>
    [SugarColumn(ColumnName = "urgentContactsName")]
    public string UrgentContactsName { get; set; }

    /// <summary>
    /// 联系人关系.
    /// </summary>
    [SugarColumn(ColumnName = "urgentContactsRelation")]
    public string UrgentContactsRelation { get; set; }

    /// <summary>
    /// 联系人电话.
    /// </summary>
    [SugarColumn(ColumnName = "urgentContactsPhone")]
    public string UrgentContactsPhone { get; set; }

}