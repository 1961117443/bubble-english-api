using QT.Common.Const;
using SqlSugar;

namespace QT.Emp.Entitys;

/// <summary>
/// 调岗管理实体.
/// </summary>
[SugarTable("emp_transfer_employee")]
[Tenant(ClaimConst.TENANTID)]
public class EmpTransferEmployeeEntity
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
    /// 员工id.
    /// </summary>
    [SugarColumn(ColumnName = "EmployeeId")]
    public string EmployeeId { get; set; }

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
    /// 调入部门id.
    /// </summary>
    [SugarColumn(ColumnName = "TransferOrganizeId")]
    public string TransferOrganizeId { get; set; }

    /// <summary>
    /// 调入岗位id.
    /// </summary>
    [SugarColumn(ColumnName = "TransferPositionId")]
    public string TransferPositionId { get; set; }

    /// <summary>
    /// 调岗原因.
    /// </summary>
    [SugarColumn(ColumnName = "Reason")]
    public string Reason { get; set; }

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

    /// <summary>
    /// 附件.
    /// </summary>
    [SugarColumn(ColumnName = "Attachment")]
    public string Attachment { get; set; }

    /// <summary>
    /// 调岗日期.
    /// </summary>
    [SugarColumn(ColumnName = "TransferTime")]
    public DateTime? TransferTime { get; set; }

}