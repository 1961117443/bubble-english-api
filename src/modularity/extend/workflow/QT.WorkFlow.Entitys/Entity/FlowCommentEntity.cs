using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.WorkFlow.Entitys.Entity;

/// <summary>
/// 流程评论.
/// </summary>
[SugarTable("FLOW_COMMENT")]
[Tenant(ClaimConst.TENANTID)]
public class FlowCommentEntity : CLDEntityBase
{
    /// <summary>
    /// 任务id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TASKID")]
    public string? TaskId { get; set; }

    /// <summary>
    /// 文本.
    /// </summary>
    [SugarColumn(ColumnName = "F_TEXT")]
    public string? Text { get; set; }

    /// <summary>
    /// 图片.
    /// </summary>
    [SugarColumn(ColumnName = "F_IMAGE")]
    public string? Image { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    [SugarColumn(ColumnName = "F_FILE")]
    public string? File { get; set; }
}
