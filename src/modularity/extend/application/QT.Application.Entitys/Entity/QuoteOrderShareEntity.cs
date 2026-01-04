using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Extend.Entitys;

/// <summary>
/// 报价单分享记录
/// </summary>
[SugarTable("EXT_QUOTE_SHARE")]
[Tenant(ClaimConst.TENANTID)]
public class QuoteOrderShareEntity : CLDEntityBase
{
    /// <summary>
    /// 报价单id.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Fid")]
    public string? Fid { get; set; }

    /// <summary>
    /// 查看密码.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_PASSWORD")]
    public string? Password { get; set; }

    /// <summary>
    /// 最多查看次数.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_MaxViewCount")]
    public int? MaxViewCount { get; set; }


    /// <summary>
    /// 查看次数.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_ViewCount")]
    public int? ViewCount { get; set; }

    /// <summary>
    /// 过期时间.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_ExpiryTime")]
    public DateTime? ExpiryTime { get; set; }
}
