using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrderLog;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class LogEnterpriseQuoteOrderLogInfoOutput: LogEnterpriseQuoteOrderLogCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
