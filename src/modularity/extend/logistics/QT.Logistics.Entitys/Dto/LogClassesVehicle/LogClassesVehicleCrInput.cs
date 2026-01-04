
namespace QT.Logistics.Entitys.Dto.LogClassesVehicle;

/// <summary>
/// 发车记录修改输入参数.
/// </summary>
public class LogClassesVehicleCrInput
{
    /// <summary>
    /// 班次ID.
    /// </summary>
    public string cId { get; set; }

    /// <summary>
    /// 车辆ID.
    /// </summary>
    public string vId { get; set; }

    /// <summary>
    /// 发车时间.
    /// </summary>
    public DateTime? departureTime { get; set; }

    /// <summary>
    /// 调度人.
    /// </summary>
    public string dispatcher { get; set; }

    /// <summary>
    /// 班次编号.
    /// </summary>
    public string code { get; set; }

}