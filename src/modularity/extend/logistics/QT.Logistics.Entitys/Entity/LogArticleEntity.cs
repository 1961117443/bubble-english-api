using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 文章系统
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("log_article")]
[Tenant(ClaimConst.TENANTID)]
public class LogArticleEntity : CLDEntityBase
{
    /// <summary>
    /// 类别：1-园区介绍，2- 招商公告.
    /// </summary>
    [SugarColumn(ColumnName = "F_TYPE")]
    public int? Type { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    [SugarColumn(ColumnName = "F_TITLE")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 正文.
    /// </summary>
    [SugarColumn(ColumnName = "F_BODYTEXT")]
    public string BodyText { get; set; }

    /// <summary>
    /// 优先.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRIORITYLEVEL")]
    public int? PriorityLevel { get; set; }

    /// <summary>
    /// 收件用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_TOUSERIDS")]
    public string ToUserIds { get; set; }

    /// <summary>
    /// 阅读次数.
    /// </summary>
    [SugarColumn(ColumnName = "F_ISREAD")]
    public int? IsRead { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    [SugarColumn(ColumnName = "F_FILES")]
    public string Files { get; set; }


    /// <summary>
    /// 唯一标识.
    /// </summary>
    [SugarColumn(ColumnName = "F_UniqueKey")]
    public string UniqueKey { get; set; }
}