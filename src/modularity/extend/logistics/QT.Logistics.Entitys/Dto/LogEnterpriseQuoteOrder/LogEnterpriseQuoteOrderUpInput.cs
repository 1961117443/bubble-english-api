using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrder;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class LogEnterpriseQuoteOrderUpInput : LogEnterpriseQuoteOrderCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
