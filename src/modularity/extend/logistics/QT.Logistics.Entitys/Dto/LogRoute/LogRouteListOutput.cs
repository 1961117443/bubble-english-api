namespace QT.Logistics.Entitys.Dto.LogRoute;

/// <summary>
/// 物流线路输入参数.
/// </summary>
public class LogRouteListOutput
{
    /// <summary>
    /// 自然主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 线路名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 线路编号.
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string description { get; set; }

}