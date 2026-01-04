using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 车辆信息实体.
/// </summary>
[SugarTable("log_vehicle_status")]
[Tenant(ClaimConst.TENANTID)]
public class LogVehicleStatusEntity : CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 车辆id
    /// </summary>
    [SugarColumn(ColumnName = "VId")]
    public string VId { get; set; }


    /// <summary>
    /// 经度.
    /// </summary>
    [SugarColumn(ColumnName = "Longitude")]
    public string Longitude { get; set; }

    /// <summary>
    /// 纬度.
    /// </summary>
    [SugarColumn(ColumnName = "Latitude")]
    public string Latitude { get; set; }

    /// <summary>
    /// 数据来源.
    /// </summary>
    [SugarColumn(ColumnName = "DataSource")]
    public string DataSource { get; set; }

    /// <summary>
    /// 采集时间
    /// </summary>
    [SugarColumn(ColumnName = "CollectionTime")]
    public DateTime? CollectionTime { get; set; }

    /// <summary>
    /// 采集设备.
    /// </summary>
    [SugarColumn(ColumnName = "CollectionDevice")]
    public string CollectionDevice { get; set; }

    /// <summary>
    /// 配送点id.
    /// </summary>
    [SugarColumn(ColumnName = "PointId")]
    public string PointId { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }
}