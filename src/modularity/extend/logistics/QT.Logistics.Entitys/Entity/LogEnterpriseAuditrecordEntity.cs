using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 入驻商家审批记录实体.
/// </summary>
[SugarTable("log_enterprise_auditrecord")]
[Tenant(ClaimConst.TENANTID)]
public class LogEnterpriseAuditrecordEntity : CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 商家ID.
    /// </summary>
    [SugarColumn(ColumnName = "EId")]
    public string EId { get; set; }

    /// <summary>
    /// 审批时间.
    /// </summary>
    [SugarColumn(ColumnName = "AuditTime")]
    public DateTime? AuditTime { get; set; }

    /// <summary>
    /// 审批用户.
    /// </summary>
    [SugarColumn(ColumnName = "AuditUserId")]
    public string AuditUserId { get; set; }

    /// <summary>
    /// 审批意见.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }

}