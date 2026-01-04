using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.WorkFlow.Entitys.Entity;

/// <summary>
/// 流程委托.
/// </summary>
[SugarTable("FLOW_DELEGATE")]
[Tenant(ClaimConst.TENANTID)]
public class FlowDelegateEntity : CLDEntityBase
{
    /// <summary>
    /// 被委托人id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TOUSERID")]
    public string? ToUserId { get; set; }

    /// <summary>
    /// 被委托人名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_TOUSERNAME")]
    public string? ToUserName { get; set; }

    /// <summary>
    /// 委托流程id.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWID")]
    public string? FlowId { get; set; }

    /// <summary>
    /// 委托流程名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWNAME")]
    public string? FlowName { get; set; }

    /// <summary>
    /// 流程分类.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWCATEGORY")]
    public string? FlowCategory { get; set; }

    /// <summary>
    /// 开始时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_STARTTIME")]
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENDTIME")]
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }
}
