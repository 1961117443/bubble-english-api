using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Extend.Entitys;

/// <summary>
/// 团队日志
/// </summary>
[SugarTable("EXT_ProjectLog")]
[Tenant(ClaimConst.TENANTID)]
public class ProjectLogEntity : CLDEntityBase
{
    /// <summary>
    /// 项目id.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_ProjectId")]
    public string? ProjectId { get; set; }

    /// <summary>
    /// 日志内容.
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
}
