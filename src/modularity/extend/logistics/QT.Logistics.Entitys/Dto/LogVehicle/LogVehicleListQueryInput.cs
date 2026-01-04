using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogVehicle;

/// <summary>
/// 车辆信息列表查询输入
/// </summary>
public class LogVehicleListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

    /// <summary>
    /// 车牌号码.
    /// </summary>
    public string licensePlateNumber { get; set; }

    /// <summary>
    /// 驾驶员.
    /// </summary>
    public string driver { get; set; }

    /// <summary>
    /// 驾驶员手机.
    /// </summary>
    public string driverPhone { get; set; }

}

public class LogVehicleStatusListQueryInput : PageInputBase
{
    /// <summary>
    /// 车辆id.
    /// </summary>
    public string vId { get; set; }


    /// <summary>
    /// 采集日期.
    /// </summary>
    public string collectionTime { get; set; }
}