using System.ComponentModel.DataAnnotations;

namespace QT.JXC.Entitys.Dto.ErpVideo;

public class IotNodeGpsDto
{
    public string id { get; set; }

    public string nodeId { get; set; }

    /// <summary>
    /// 报警标识
    /// </summary>
    public int? alarmFlag { get; set; }

    /// <summary>
    /// 采集时间
    /// </summary>
    public DateTime? gpsTime { get; set; }

    /// <summary>
    /// 纬度值乘以10^6（实际纬度 = Lat / 1_000_000.0）
    /// </summary>
    public int? lat { get; set; }

    /// <summary>
    /// 经度值乘以10^6（实际经度 = Lng / 1_000_000.0）
    /// </summary>
    public int? lng { get; set; }

    /// <summary>
    /// 方向 0-359（正北为0，顺时针）
    /// </summary>
    public int? direction { get; set; }

    /// <summary>
    /// 速度（实际速度 = Speed * 0.1 km/h）
    /// </summary>
    public double? speed { get; set; }

    /// <summary>
    /// 状态位标志
    /// </summary>
    public int? statusFlag { get; set; }

    /// <summary>
    /// 扩展属性
    /// </summary>
    public string extend { get; set; }
}

public class IotNodeGpsQueryInput
{
    /// <summary>
    /// 设备id
    /// </summary>
    [Required]
    public string deviceId { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? startTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? endTime { get; set; }

    /// <summary>
    /// 停留时间，计算行程
    /// </summary>
    public int? maxParkingMinutes { get; set; }
}
