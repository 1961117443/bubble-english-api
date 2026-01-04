using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 发车记录实体.
/// </summary>
[SugarTable("log_classes_vehicle")]
[Tenant(ClaimConst.TENANTID)]
public class LogClassesVehicleEntity : CLDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 班次ID.
    /// </summary>
    [SugarColumn(ColumnName = "CId")]
    public string CId { get; set; }

    /// <summary>
    /// 车辆ID.
    /// </summary>
    [SugarColumn(ColumnName = "VId")]
    public string VId { get; set; }

    /// <summary>
    /// 发车时间.
    /// </summary>
    [SugarColumn(ColumnName = "DepartureTime")]
    public DateTime? DepartureTime { get; set; }

    /// <summary>
    /// 调度人.
    /// </summary>
    [SugarColumn(ColumnName = "Dispatcher")]
    public string Dispatcher { get; set; }

    /// <summary>
    /// 班次编号.
    /// </summary>
    [SugarColumn(ColumnName = "Code")]
    public string Code { get; set; }
}