using QT.Common.Const;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 建筑人才求职信息实体.
/// </summary>
[SugarTable("jzrc_talent_job")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcTalentJobEntity
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
    /// 人才id.
    /// </summary>
    [SugarColumn(ColumnName = "TalentId")]
    public string TalentId { get; set; }

    /// <summary>
    /// 招聘地区.
    /// </summary>
    [SugarColumn(ColumnName = "Region")]
    public string Region { get; set; }

    /// <summary>
    /// 求职类型.
    /// </summary>
    [SugarColumn(ColumnName = "CandidateType")]
    public string CandidateType { get; set; }

    /// <summary>
    /// 薪资要求（年薪）.
    /// </summary>
    [SugarColumn(ColumnName = "JobSalary")]
    public decimal JobSalary { get; set; }

    /// <summary>
    /// 社保情况.
    /// </summary>
    [SugarColumn(ColumnName = "SocialSecurityStatus")]
    public string SocialSecurityStatus { get; set; }

    /// <summary>
    /// 业绩情况.
    /// </summary>
    [SugarColumn(ColumnName = "PerformanceSituation")]
    public string PerformanceSituation { get; set; }

    /// <summary>
    /// 证书id.
    /// </summary>
    [SugarColumn(ColumnName = "CertificateId")]
    public string CertificateId { get; set; }

    /// <summary>
    /// 求职时间起.
    /// </summary>
    [SugarColumn(ColumnName = "RequiredStart")]
    public DateTime? RequiredStart { get; set; }

    /// <summary>
    /// 求职时间止.
    /// </summary>
    [SugarColumn(ColumnName = "RequiredEnd")]
    public DateTime? RequiredEnd { get; set; }

    /// <summary>
    /// 一口价.
    /// </summary>
    [SugarColumn(ColumnName = "Price")]
    public decimal Price { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [SugarColumn(ColumnName = "Status")]
    public int? Status { get; set; }

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
    /// 创建用户名称.
    /// </summary>
    [SugarColumn(ColumnName = "CreatorUserName")]
    public string CreatorUserName { get; set; }

    /// <summary>
    /// 审核用户名称.
    /// </summary>
    [SugarColumn(ColumnName = "AuditUserName")]
    public string AuditUserName { get; set; }

}