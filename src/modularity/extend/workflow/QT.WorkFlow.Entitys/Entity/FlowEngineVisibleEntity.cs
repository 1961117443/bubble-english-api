using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.WorkFlow.Entitys.Entity;

/// <summary>
/// 流程可见.
/// </summary>
[SugarTable("FLOW_ENGINEVISIBLE")]
[Tenant(ClaimConst.TENANTID)]
public class FlowEngineVisibleEntity : CEntityBase
{
    /// <summary>
    /// 流程主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWID")]
    public string? FlowId { get; set; }

    /// <summary>
    /// 经办类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_OPERATORTYPE")]
    public string? OperatorType { get; set; }

    /// <summary>
    /// 经办主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_OPERATORID")]
    public string? OperatorId { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }
}
