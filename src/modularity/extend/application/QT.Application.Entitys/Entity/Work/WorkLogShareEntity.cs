using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Extend.Entitys;

/// <summary>
/// 工作日志分享




/// </summary>
[SugarTable("EXT_WORKLOGSHARE")]
[Tenant(ClaimConst.TENANTID)]
public class WorkLogShareEntity : EntityBase<string>
{
    /// <summary>
    /// 日志主键.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_WORKLOGID")]
    public string? WorkLogId { get; set; }

    /// <summary>
    /// 共享人员.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_SHAREUSERID")]
    public string? ShareUserId { get; set; }

    /// <summary>
    /// 共享时间.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_SHARETIME")]
    public DateTime? ShareTime { get; set; }
}
