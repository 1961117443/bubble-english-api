namespace QT.Logistics.Entitys.Dto.LogClasses;

/// <summary>
/// 班次信息输入参数.
/// </summary>
public class LogClassesListOutput
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
    /// 线路.
    /// </summary>
    public string routeIdName { get; set; }

}