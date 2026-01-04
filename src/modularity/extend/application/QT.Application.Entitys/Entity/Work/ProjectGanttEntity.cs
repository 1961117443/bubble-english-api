using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Extend.Entitys;

/// <summary>
/// 项目计划
/// </summary>
[SugarTable("EXT_PROJECTGANTT")]
[Tenant(ClaimConst.TENANTID)]
public class ProjectGanttEntity : CLDEntityBase
{
    /// <summary>
    /// 项目上级.
    /// </summary>
    [SugarColumn(ColumnName = "F_PARENTID")]
    public string? ParentId { get; set; }

    /// <summary>
    /// 项目主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_PROJECTID")]
    public string? ProjectId { get; set; }

    /// <summary>
    /// 项目类型：【1-项目、2-任务】.
    /// </summary>
    [SugarColumn(ColumnName = "F_TYPE")]
    public int? Type { get; set; }

    /// <summary>
    /// 项目编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE")]
    public string? EnCode { get; set; }

    /// <summary>
    /// 项目名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string? FullName { get; set; }

    /// <summary>
    /// 项目工期.
    /// </summary>
    [SugarColumn(ColumnName = "F_TIMELIMIT")]
    public decimal? TimeLimit { get; set; }

    /// <summary>
    /// 项目标记.
    /// </summary>
    [SugarColumn(ColumnName = "F_SIGN")]
    public string? Sign { get; set; }

    /// <summary>
    /// 标记颜色.
    /// </summary>
    [SugarColumn(ColumnName = "F_SIGNCOLOR")]
    public string? SignColor { get; set; }

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
    /// 当前进度.
    /// </summary>
    [SugarColumn(ColumnName = "F_SCHEDULE")]
    public int? Schedule { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    [SugarColumn(ColumnName = "F_MANAGERIDS")]
    public string? ManagerIds { get; set; }

    /// <summary>
    /// 参与人.
    /// </summary>
    [SugarColumn(ColumnName = "F_MEMBERIDS")]
    public string? MemberIds { get; set; }

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

    /// <summary>
    /// 项目状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_STATE")]
    public int? State { get; set; }
}
