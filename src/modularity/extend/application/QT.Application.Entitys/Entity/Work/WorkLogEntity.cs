using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Extend.Entitys;

/// <summary>
/// 工作日志




/// </summary>
[SugarTable("EXT_WORKLOG")]
[Tenant(ClaimConst.TENANTID)]
public class WorkLogEntity : CLDEntityBase
{
    /// <summary>
    /// 日志标题.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_TITLE")]
    public string? Title { get; set; }

    /// <summary>
    /// 今天内容.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_TODAYCONTENT")]
    public string? TodayContent { get; set; }

    /// <summary>
    /// 明天内容.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_TOMORROWCONTENT")]
    public string? TomorrowContent { get; set; }

    /// <summary>
    /// 遇到问题.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_QUESTION")]
    public string? Question { get; set; }

    /// <summary>
    /// 发送给谁.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_TOUSERID")]
    public string? ToUserId { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

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
    /// 日志分类.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Category")]
    public int? Category { get; set; }
}
