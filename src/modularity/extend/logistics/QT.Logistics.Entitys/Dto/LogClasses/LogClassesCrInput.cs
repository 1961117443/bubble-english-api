
namespace QT.Logistics.Entitys.Dto.LogClasses;

/// <summary>
/// 班次信息修改输入参数.
/// </summary>
public class LogClassesCrInput
{
    /// <summary>
    /// 班次名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 发车时间.
    /// </summary>
    public DateTime? departureTime { get; set; }

    /// <summary>
    /// 到达时间.
    /// </summary>
    public DateTime? arrivalTime { get; set; }

    /// <summary>
    /// 线路ID.
    /// </summary>
    public string routeId { get; set; }

    /// <summary>
    /// 关联车辆
    /// </summary>
    public List<string> vIdList { get; set; }
}