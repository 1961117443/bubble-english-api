
namespace QT.Logistics.Entitys.Dto.LogClasses;

/// <summary>
/// 班次信息输出参数.
/// </summary>
public class LogClassesInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 班次名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 发车时间.
    /// </summary>
    public string departureTime { get; set; }

    /// <summary>
    /// 到达时间.
    /// </summary>
    public string arrivalTime { get; set; }

    /// <summary>
    /// 线路ID.
    /// </summary>
    public string routeId { get; set; }

    /// <summary>
    /// 已关联车辆
    /// </summary>
    public IEnumerable<string> vIdList { get; set; } = new List<string>();
}