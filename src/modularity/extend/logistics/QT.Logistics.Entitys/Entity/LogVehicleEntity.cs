using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 车辆信息实体.
/// </summary>
[SugarTable("log_vehicle")]
[Tenant(ClaimConst.TENANTID)]
public class LogVehicleEntity : CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 车牌号码.
    /// </summary>
    [SugarColumn(ColumnName = "LicensePlateNumber")]
    public string LicensePlateNumber { get; set; }

    /// <summary>
    /// 尺寸.
    /// </summary>
    [SugarColumn(ColumnName = "Size")]
    public string Size { get; set; }

    /// <summary>
    /// 运送类型.
    /// </summary>
    [SugarColumn(ColumnName = "TransportType")]
    public string TransportType { get; set; }

    /// <summary>
    /// 吨位.
    /// </summary>
    [SugarColumn(ColumnName = "Tone")]
    public string Tone { get; set; }

    /// <summary>
    /// 驾驶员.
    /// </summary>
    [SugarColumn(ColumnName = "Driver")]
    public string Driver { get; set; }

    /// <summary>
    /// 驾驶员手机.
    /// </summary>
    [SugarColumn(ColumnName = "DriverPhone")]
    public string DriverPhone { get; set; }

    /// <summary>
    /// 运送状态.
    /// </summary>
    [SugarColumn(ColumnName = "TransportStatus")]
    public string TransportStatus { get; set; }


    /// <summary>
    /// 图片.
    /// </summary>
    [SugarColumn(ColumnName = "ImageUrl")]
    public string ImageUrl { get; set; }
}
