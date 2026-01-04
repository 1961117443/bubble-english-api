using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogOrderFinancialConfiguration;

/// <summary>
/// 订单分账配置表列表查询输入
/// </summary>
public class LogOrderFinancialConfigurationListQueryInput : PageInputBase
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
    /// 配置名称.
    /// </summary>
    public string name { get; set; }

}