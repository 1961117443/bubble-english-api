using QT.Common.Const;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 车辆班次关联表.
/// </summary>
[SugarTable("log_vehicle_classes")]
[Tenant(ClaimConst.TENANTID)]
public class LogVehicleClassesEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 车辆ID.
    /// </summary>
    [SugarColumn(ColumnName = "VId")]
    public string VId { get; set; }

    /// <summary>
    /// 班次ID.
    /// </summary>
    [SugarColumn(ColumnName = "CId")]
    public string CId { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorTime")]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 创建用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorUserId")]
    public string CreatorUserId { get; set; }

}