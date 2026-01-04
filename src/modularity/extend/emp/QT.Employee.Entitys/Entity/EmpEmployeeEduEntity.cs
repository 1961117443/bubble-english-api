using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Emp.Entitys;

/// <summary>
/// 员工学历实体.
/// </summary>
[SugarTable("emp_employee_edu")]
[Tenant(ClaimConst.TENANTID)]
public class EmpEmployeeEduEntity : CUDEntityBase
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
    /// 学历.
    /// </summary>
    [SugarColumn(ColumnName = "HighestEdu")]
    public string HighestEdu { get; set; }

    /// <summary>
    /// 毕业院校.
    /// </summary>
    [SugarColumn(ColumnName = "GraduateSchool")]
    public string GraduateSchool { get; set; }

    /// <summary>
    /// 毕业时间.
    /// </summary>
    [SugarColumn(ColumnName = "GraduationTime")]
    public DateTime? GraduationTime { get; set; }

    /// <summary>
    /// 所学专业.
    /// </summary>
    [SugarColumn(ColumnName = "Major")]
    public string Major { get; set; }

}