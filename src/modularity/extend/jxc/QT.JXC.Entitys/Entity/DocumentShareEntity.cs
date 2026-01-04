using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JXC.Entitys.Entity;

/// <summary>
/// 知识文档共享
/// </summary>
[SugarTable("EXT_DOCUMENTSHARE")]
[Tenant(ClaimConst.TENANTID)]
public class DocumentShareEntity : EntityBase<string>
{
    /// <summary>
    /// 文档主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_DOCUMENTID")]
    public string? DocumentId { get; set; }

    /// <summary>
    /// 共享人员.
    /// </summary>
    [SugarColumn(ColumnName = "F_SHAREUSERID")]
    public string? ShareUserId { get; set; }

    /// <summary>
    /// 共享时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_SHARETIME")]
    public DateTime? ShareTime { get; set; }
}
