using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 人才沟通记录实体.
/// </summary>
[SugarTable("jzrc_talent_handover")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcTalentHandoverEntity: CUDEntityBase
{
    /// <summary>
    /// 人才id.
    /// </summary>
    [SugarColumn(ColumnName = "TalentId")]
    public string TalentId { get; set; }


    /// <summary>
    /// 沟通时间.
    /// </summary>
    [SugarColumn(ColumnName = "HandoverTime")]
    public DateTime? HandoverTime { get; set; }

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