using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 班次信息实体.
/// </summary>
[SugarTable("log_classes")]
[Tenant(ClaimConst.TENANTID)]
public class LogClassesEntity : CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 班次名称.
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// 发车时间.
    /// </summary>
    [SugarColumn(ColumnName = "DepartureTime")]
    public DateTime? DepartureTime { get; set; }

    /// <summary>
    /// 到达时间.
    /// </summary>
    [SugarColumn(ColumnName = "ArrivalTime")]
    public DateTime? ArrivalTime { get; set; }

    /// <summary>
    /// 线路ID.
    /// </summary>
    [SugarColumn(ColumnName = "RouteId")]
    public string RouteId { get; set; }
}