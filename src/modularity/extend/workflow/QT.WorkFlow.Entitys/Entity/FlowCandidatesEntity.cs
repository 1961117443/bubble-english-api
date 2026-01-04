using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.WorkFlow.Entitys.Entity;

/// <summary>
/// 流程候选人.
/// </summary>
[SugarTable("FLOW_CANDIDATES")]
[Tenant(ClaimConst.TENANTID)]
public class FlowCandidatesEntity : EntityBase<string>
{
    /// <summary>
    /// 任务id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TASKID")]
    public string? TaskId { get; set; }

    /// <summary>
    /// 节点id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TASKNODEID")]
    public string? TaskNodeId { get; set; }

    /// <summary>
    /// 审批人id.
    /// </summary>
    [SugarColumn(ColumnName = "F_HANDLEID")]
    public string? HandleId { get; set; }

    /// <summary>
    /// 审批人账号.
    /// </summary>
    [SugarColumn(ColumnName = "F_ACCOUNT")]
    public string? Account { get; set; }

    /// <summary>
    /// 候选人.
    /// </summary>
    [SugarColumn(ColumnName = "F_CANDIDATES")]
    public string? Candidates { get; set; }

    /// <summary>
    /// 经办id.
    /// </summary>
    [SugarColumn(ColumnName = "F_TASKOPERATORID")]
    public string? TaskOperatorId { get; set; }
}
