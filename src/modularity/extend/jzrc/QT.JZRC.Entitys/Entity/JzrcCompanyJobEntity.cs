using NPOI.Util;
using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 企业招聘职位实体.
/// </summary>
[SugarTable("jzrc_company_job")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcCompanyJobEntity: CUDEntityBase
{
    /// <summary>
    /// 企业id.
    /// </summary>
    [SugarColumn(ColumnName = "CompanyId")]
    public string CompanyId { get; set; }

    /// <summary>
    /// 职位名称.
    /// </summary>
    [SugarColumn(ColumnName = "JobTitle")]
    public string JobTitle { get; set; }

    /// <summary>
    /// 人才类型.
    /// </summary>
    [SugarColumn(ColumnName = "CandidateType")]
    public string CandidateType { get; set; }

    /// <summary>
    /// 招聘数量.
    /// </summary>
    [SugarColumn(ColumnName = "Number")]
    public int? Number { get; set; }

    /// <summary>
    /// 职位薪资.
    /// </summary>
    [SugarColumn(ColumnName = "JobSalary")]
    public int JobSalary { get; set; }

    /// <summary>
    /// 需求时间起.
    /// </summary>
    [SugarColumn(ColumnName = "RequiredStart")]
    public DateTime? RequiredStart { get; set; }

    /// <summary>
    /// 需求时间止.
    /// </summary>
    [SugarColumn(ColumnName = "RequiredEnd")]
    public DateTime? RequiredEnd { get; set; }

    /// <summary>
    /// 招聘地区.
    /// </summary>
    [SugarColumn(ColumnName = "Region")]
    public string Region { get; set; }

    /// <summary>
    /// 证书类型.
    /// </summary>
    [SugarColumn(ColumnName = "CertificateCategoryId")]
    public string CertificateCategoryId { get; set; }

    /// <summary>
    /// 证书等级.
    /// </summary>
    [SugarColumn(ColumnName = "CertificateLevel")]
    public string CertificateLevel { get; set; }

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
    /// 一口价.
    /// </summary>
    [SugarColumn(ColumnName = "Price")]
    public int Price { get; set; }

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
    /// 创建用户名称
    /// </summary>
    [SugarColumn(ColumnName = "CreatorUserName")]
    public string CreatorUserName { get; set; }

    /// <summary>
    /// 审核用户名称
    /// </summary>
    [SugarColumn(ColumnName = "AuditUserName")]
    public string AuditUserName { get; set; }
}