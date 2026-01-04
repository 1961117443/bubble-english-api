using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Extend.Entitys;

/// <summary>
/// 工单管理
/// </summary>
[SugarTable("EXT_QUOTE_LOG")]
[Tenant(ClaimConst.TENANTID)]
public class QuoteOrderLogEntity : CLDEntityBase
{
    /// <summary>
    /// 报价单id.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Fid")]
    public string? Fid { get; set; }

    /// <summary>
    /// 问题描述.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_CONTENT")]
    public string? Content { get; set; }

    /// <summary>
    /// 图片.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_ImageJson")]
    public string? ImageJson { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_AttachJson")]
    public string? AttachJson { get; set; }


    /// <summary>
    /// 是否已读.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Read")]
    public int? Read { get; set; }

    /// <summary>
    /// 收件人id.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_ReceiveUserId")]
    public string? ReceiveUserId { get; set; }
}
