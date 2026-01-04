using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 企业签约人才实体.
/// </summary>
[SugarTable("jzrc_company_talent")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcCompanyTalentEntity: CUDEntityBase
{
    /// <summary>
    /// 企业id.
    /// </summary>
    [SugarColumn(ColumnName = "CompanyId")]
    public string CompanyId { get; set; }

    /// <summary>
    /// 人才id.
    /// </summary>
    [SugarColumn(ColumnName = "TalentId")]
    public string TalentId { get; set; }

    /// <summary>
    /// 签约时间.
    /// </summary>
    [SugarColumn(ColumnName = "SigningDate")]
    public DateTime? SigningDate { get; set; }

    /// <summary>
    /// 解约时间.
    /// </summary>
    [SugarColumn(ColumnName = "TerminationDate")]
    public DateTime? TerminationDate { get; set; }

}