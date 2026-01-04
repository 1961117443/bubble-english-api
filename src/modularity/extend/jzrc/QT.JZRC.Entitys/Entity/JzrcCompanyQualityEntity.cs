using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 企业资质信息实体.
/// </summary>
[SugarTable("jzrc_company_quality")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcCompanyQualityEntity: CUDEntityBase
{
    /// <summary>
    /// 企业id.
    /// </summary>
    [SugarColumn(ColumnName = "CompanyId")]
    public string CompanyId { get; set; }

    /// <summary>
    /// 资质名称.
    /// </summary>
    [SugarColumn(ColumnName = "QualificationName")]
    public string QualificationName { get; set; }

    /// <summary>
    /// 等级.
    /// </summary>
    [SugarColumn(ColumnName = "Level")]
    public string Level { get; set; }

    /// <summary>
    /// 专业.
    /// </summary>
    [SugarColumn(ColumnName = "Specialty")]
    public string Specialty { get; set; }

    /// <summary>
    /// 颁发时间.
    /// </summary>
    [SugarColumn(ColumnName = "IssuanceDate")]
    public DateTime? IssuanceDate { get; set; }

    /// <summary>
    /// 颁发机构.
    /// </summary>
    [SugarColumn(ColumnName = "IssuingAuthority")]
    public string IssuingAuthority { get; set; }

    /// <summary>
    /// 有效期限.
    /// </summary>
    [SugarColumn(ColumnName = "ValidityPeriod")]
    public string ValidityPeriod { get; set; }

}