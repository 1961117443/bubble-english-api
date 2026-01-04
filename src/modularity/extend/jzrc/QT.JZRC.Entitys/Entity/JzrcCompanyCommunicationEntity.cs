using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 企业沟通记录实体.
/// </summary>
[SugarTable("jzrc_company_communication")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcCompanyCommunicationEntity: CUDEntityBase
{
    /// <summary>
    /// 企业id.
    /// </summary>
    [SugarColumn(ColumnName = "CompanyId")]
    public string CompanyId { get; set; }

    /// <summary>
    /// 客户经理id.
    /// </summary>
    [SugarColumn(ColumnName = "ManagerId")]
    public string ManagerId { get; set; }

    /// <summary>
    /// 沟通时间.
    /// </summary>
    [SugarColumn(ColumnName = "CommunicationTime")]
    public DateTime? CommunicationTime { get; set; }

    /// <summary>
    /// 是否接通.
    /// </summary>
    [SugarColumn(ColumnName = "WhetherContent")]
    public int? WhetherContent { get; set; }

    /// <summary>
    /// 沟通内容.
    /// </summary>
    [SugarColumn(ColumnName = "Content")]
    public string Content { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    [SugarColumn(ColumnName = "Attachment")]
    public string Attachment { get; set; }

}