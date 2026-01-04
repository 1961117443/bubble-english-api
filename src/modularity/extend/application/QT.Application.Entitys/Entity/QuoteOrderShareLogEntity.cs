using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Extend.Entitys;

/// <summary>
/// 报价单分享浏览记录
/// </summary>
[SugarTable("EXT_QUOTE_SHARE_LOG")]
[Tenant(ClaimConst.TENANTID)]
public class QuoteOrderShareLogEntity : CLDEntityBase
{
    /// <summary>
    /// 报价单id
    /// </summary>
    [SugarColumn(ColumnName = "F_Fid")]
    public string Fid { get; set; }

    /// <summary>
    /// 分享id
    /// </summary>
    [SugarColumn(ColumnName = "F_Sid")]
    public string Sid { get; set; }

    /// <summary>
    /// 报价单id
    /// </summary>
    [SugarColumn(ColumnName = "F_Ip")]
    public string Ip { get; set; }
}