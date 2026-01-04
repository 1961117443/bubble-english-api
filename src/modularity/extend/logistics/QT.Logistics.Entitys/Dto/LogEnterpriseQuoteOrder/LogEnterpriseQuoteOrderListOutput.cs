using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrder;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class LogEnterpriseQuoteOrderListOutput: LogEnterpriseQuoteOrderInfoOutput
{ 
    /// <summary>
    /// 客户名称
    /// </summary>
    public string cidName { get; set; }

    /// <summary>
    /// 客户电话
    /// </summary>
    public string cidAdmintel { get;set; }

    /// <summary>
    /// 浏览次数
    /// </summary>
    public int? viewCount { get; set; }

    /// <summary>
    /// 最后浏览时间
    /// </summary>
    public DateTime? lastViewTime { get; set; }
}
