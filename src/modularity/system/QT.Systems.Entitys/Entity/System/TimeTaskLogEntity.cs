using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.System;

/// <summary>
/// 定时任务日志




/// </summary>
//[SugarTable("BASE_TIMETASKLOG")]
[SugarTable("BASE_TIMETASKLOG_{year}{month}{day}")]
[Tenant(ClaimConst.TENANTID)]
[SplitTable(SplitType.Month)]
[SugarIndex("runtime", nameof(TimeTaskLogEntity.RunTime), OrderByType.Desc)]
public class TimeTaskLogEntity : EntityBase<string>
{
    [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", IsPrimaryKey = true, Length = 50, ColumnDataType = "bigint")]
    public override string Id { get; set; }
    /// <summary>
    /// 定时任务主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_TASKID", IsNullable = true, Length = 50)]
    public string TaskId { get; set; }

    /// <summary>
    /// 执行时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_RUNTIME",IsNullable =true)]
    [SplitField]
    public DateTime? RunTime { get; set; }

    /// <summary>
    /// 执行结果.
    /// </summary>
    [SugarColumn(ColumnName = "F_RUNRESULT", IsNullable = true)]
    public int? RunResult { get; set; }

    /// <summary>
    /// 执行说明.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION", IsNullable = true, ColumnDataType = "longtext")]
    public string Description { get; set; }
}