using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogVehicle;

/// <summary>
/// 车辆信息输入参数.
/// </summary>
public class LogVehicleListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 车牌号码.
    /// </summary>
    public string licensePlateNumber { get; set; }

    /// <summary>
    /// 尺寸.
    /// </summary>
    public string size { get; set; }

    /// <summary>
    /// 运送类型.
    /// </summary>
    public string transportType { get; set; }

    /// <summary>
    /// 吨位.
    /// </summary>
    public string tone { get; set; }

    /// <summary>
    /// 驾驶员.
    /// </summary>
    public string driver { get; set; }

    /// <summary>
    /// 驾驶员手机.
    /// </summary>
    public string driverPhone { get; set; }

    /// <summary>
    /// 运送状态.
    /// </summary>
    public string transportStatus { get; set; }

}

/// <summary>
/// 车辆监控信息输出
/// </summary>
[SuppressSniffer]
public class LogVehicleStatusListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 车辆id.
    /// </summary>
    public string vId { get; set; }


    /// <summary>
    /// 车辆
    /// </summary>
    public string vIdName { get; set; }

    /// <summary>
    /// 经度.
    /// </summary>
    public string longitude { get; set; }

    /// <summary>
    /// 纬度.
    /// </summary>
    public string latitude { get; set; }

    /// <summary>
    /// 数据来源.
    /// </summary>
    public string dateSource { get; set; }

    /// <summary>
    /// 采集时间.
    /// </summary>
    public DateTime? collectionTime { get; set; }

    /// <summary>
    /// 采集设备.
    /// </summary>
    public string collectionDevice { get; set; }

    /// <summary>
    /// 配送点.
    /// </summary>
    public string pointId { get; set; }

    /// <summary>
    /// 配送点.
    /// </summary>
    public string pointIdName { get; set; }
}