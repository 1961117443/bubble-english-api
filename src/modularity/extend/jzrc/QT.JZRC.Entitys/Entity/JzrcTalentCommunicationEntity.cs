using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 人才沟通记录实体.
/// </summary>
[SugarTable("jzrc_talent_communication")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcTalentCommunicationEntity: CUDEntityBase
{
    /// <summary>
    /// 人才id.
    /// </summary>
    [SugarColumn(ColumnName = "TalentId")]
    public string TalentId { get; set; }

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