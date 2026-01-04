using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Emp.Entitys;

/// <summary>
/// 面试管理实体.
/// </summary>
[SugarTable("emp_interview_employee")]
[Tenant(ClaimConst.TENANTID)]
public class EmpInterviewEmployeeEntity :IDeleteTime
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorTime")]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 创建用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorUserId")]
    public string CreatorUserId { get; set; }

    /// <summary>
    /// 修改时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyTime")]
    public DateTime? LastModifyTime { get; set; }

    /// <summary>
    /// 修改用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyUserId")]
    public string LastModifyUserId { get; set; }

    /// <summary>
    /// 删除标志.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteMark")]
    public int? DeleteMark { get; set; }

    /// <summary>
    /// 删除时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteTime")]
    public DateTime? DeleteTime { get; set; }

    /// <summary>
    /// 删除用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteUserId")]
    public string DeleteUserId { get; set; }

    /// <summary>
    /// 姓名.
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// 手机号码.
    /// </summary>
    [SugarColumn(ColumnName = "Mobile")]
    public string Mobile { get; set; }

    /// <summary>
    /// 部门id.
    /// </summary>
    [SugarColumn(ColumnName = "OrganizeId")]
    public string OrganizeId { get; set; }

    /// <summary>
    /// 岗位id.
    /// </summary>
    [SugarColumn(ColumnName = "PositionId")]
    public string PositionId { get; set; }

    /// <summary>
    /// 预计面试时间.
    /// </summary>
    [SugarColumn(ColumnName = "InterviewTime")]
    public DateTime? InterviewTime { get; set; }


    /// <summary>
    /// 审核时间.
    /// </summary>
    [SugarColumn(ColumnName = "AuditTime")]
    public DateTime? AuditTime { get; set; }

    /// <summary>
    /// 审核用户.
    /// </summary>
    [SugarColumn(ColumnName = "AuditUserId")]
    public string AuditUserId { get; set; }

    ///// <summary>
    ///// 员工类型.
    ///// </summary>
    //[SugarColumn(ColumnName = "EmployeeType")]
    //public string EmployeeType { get; set; }

    ///// <summary>
    ///// 员工状态.
    ///// </summary>
    //[SugarColumn(ColumnName = "EmployeeStatus")]
    //public string EmployeeStatus { get; set; }


    /// <summary>
    /// 预计入职时间.
    /// </summary>
    [SugarColumn(ColumnName = "ConfrimJoinTime")]
    public DateTime? ConfrimJoinTime { get; set; }

    /// <summary>
    /// 面试结果（通过、不通过）.
    /// </summary>
    [SugarColumn(ColumnName = "Result")]
    public string Result { get; set; }


    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }
}