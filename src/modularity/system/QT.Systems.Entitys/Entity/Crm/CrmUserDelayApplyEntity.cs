using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.Crm;

/// <summary>
/// CRM体验用户延期申请.
/// </summary>
[SugarTable("crm_user_delay_apply")]
[Tenant(ClaimConst.TENANTID)]
public class CrmUserDelayApplyEntity : CUDEntityBase
{
    /// <summary>
    /// 用户id.
    /// </summary>
    [SugarColumn(ColumnName = "UserId")]
    public string UserId { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [SugarColumn(ColumnName = "Status")]
    public int Status { get; set; } = 0;

    /// <summary>
    /// 到期时间.
    /// </summary>
    [SugarColumn(ColumnName = "ExpireTime")]
    public DateTime? ExpireTime { get; set; }


    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Content")]
    public string Content { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    [SugarColumn(ColumnName = "Attachment")]
    public string Attachment { get; set; }

}