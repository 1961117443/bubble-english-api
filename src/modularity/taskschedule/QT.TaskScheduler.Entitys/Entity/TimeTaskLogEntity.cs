using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.TaskScheduler.Entitys.Entity;

/// <summary>
/// 定时任务日志




/// </summary>
//[SugarTable("BASE_TIMETASKLOG")]
//[Tenant(ClaimConst.TENANTID)]
[SugarTable("BASE_TIMETASKLOG_{year}{month}{day}")]
[Tenant(ClaimConst.TENANTID)]
[SplitTable(SplitType.Month)]
public class TimeTaskLogEntity : EntityBase<string>
{
    /// <summary>
    /// 定时任务主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_TASKID")]
    public string TaskId { get; set; }

    /// <summary>
    /// 执行时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_RUNTIME")]
    [SplitField]
    public DateTime? RunTime { get; set; }

    /// <summary>
    /// 执行结果.
    /// </summary>
    [SugarColumn(ColumnName = "F_RUNRESULT")]
    public int? RunResult { get; set; }

    /// <summary>
    /// 执行说明.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }
}
