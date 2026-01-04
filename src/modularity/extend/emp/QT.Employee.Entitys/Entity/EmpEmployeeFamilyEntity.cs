using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Emp.Entitys;

/// <summary>
/// 员工家庭信息实体.
/// </summary>
[SugarTable("emp_employee_family")]
[Tenant(ClaimConst.TENANTID)]
public class EmpEmployeeFamilyEntity :CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 员工ID.
    /// </summary>
    [SugarColumn(ColumnName = "EmployeeId")]
    public string EmployeeId { get; set; }

    /// <summary>
    /// 姓名.
    /// </summary>
    [SugarColumn(ColumnName = "MemberName")]
    public string MemberName { get; set; }

    /// <summary>
    /// 关系.
    /// </summary>
    [SugarColumn(ColumnName = "MemberRelation")]
    public string MemberRelation { get; set; }

    /// <summary>
    /// 性别.
    /// </summary>
    [SugarColumn(ColumnName = "MemberGender")]
    public string MemberGender { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    [SugarColumn(ColumnName = "MemberPhone")]
    public string MemberPhone { get; set; }

    /// <summary>
    /// 生日.
    /// </summary>
    [SugarColumn(ColumnName = "MemberBirthday")]
    public DateTime? MemberBirthday { get; set; }

}