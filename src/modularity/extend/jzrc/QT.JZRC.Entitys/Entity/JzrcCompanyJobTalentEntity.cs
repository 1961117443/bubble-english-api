using QT.Common.Const;
using QT.Common.Contracts;
using QT.JZRC.Entitys.Dto.AppService;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 企业签约人才实体.
/// </summary>
[SugarTable("jzrc_company_job_talent")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcCompanyJobTalentEntity: CUDEntityBase
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
    /// 岗位id或者职位id.
    /// </summary>
    [SugarColumn(ColumnName = "JobId")]
    public string JobId { get; set; }

    /// <summary>
    /// 金额.
    /// </summary>
    [SugarColumn(ColumnName = "Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 状态(0：报名，1：签约).
    /// </summary>
    [SugarColumn(ColumnName = "Status")]
    public int Status { get; set; }


    /// <summary>
    /// 类型（0：人才，1：企业）.
    /// </summary>
    [SugarColumn(ColumnName = "Category")]
    public AppLoginUserRole Category { get; set; }

    /// <summary>
    /// 佣金.
    /// </summary>
    [SugarColumn(ColumnName = "Commission")]
    public decimal Commission { get; set; }

    /// <summary>
    /// 佣金比例.
    /// </summary>
    [SugarColumn(ColumnName = "CommissionRate")]
    public decimal CommissionRate { get; set; }
}