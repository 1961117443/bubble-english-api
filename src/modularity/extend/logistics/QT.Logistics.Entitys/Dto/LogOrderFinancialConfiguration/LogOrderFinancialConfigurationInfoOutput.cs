
namespace QT.Logistics.Entitys.Dto.LogOrderFinancialConfiguration;

/// <summary>
/// 订单分账配置表输出参数.
/// </summary>
public class LogOrderFinancialConfigurationInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 配置名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 平台占比.
    /// </summary>
    public decimal platformProportion { get; set; }

    /// <summary>
    /// 收件点占比.
    /// </summary>
    public decimal pointProportion { get; set; }

    /// <summary>
    /// 启用状态.
    /// </summary>
    public int? status { get; set; }

    /// <summary>
    /// 配置说明.
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// 到达点占比.
    /// </summary>
    public decimal reachPointProportion { get; set; }

}