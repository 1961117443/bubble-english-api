using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.WorkFlow.Entitys.Entity;

/// <summary>
/// 流程传阅.
/// </summary>
[SugarTable("FLOW_TASKCIRCULATE")]
[Tenant(ClaimConst.TENANTID)]
public class FlowTaskCirculateEntity : EntityBase<string>, ICreatorTime
{
    /// <summary>
    /// 对象类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_OBJECTTYPE")]
    public string? ObjectType { get; set; }

    /// <summary>
    /// 对象主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_OBJECTID")]
    public string? ObjectId { get; set; }

    /// <summary>
    /// 节点编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_NODECODE")]
    public string? NodeCode { get; set; }

    /// <summary>
    /// 节点名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_NODENAME")]
    public string? NodeName { get; set; }

    /// <summary>
    /// 节点主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_TASKNODEID")]
    public string? TaskNodeId { get; set; }

    /// <summary>
    /// 任务主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_TASKID")]
    public string? TaskId { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CREATORTIME")]
    public DateTime? CreatorTime { get; set; }
}
