using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrderShare;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class LogEnterpriseQuoteOrderShareUpInput : LogEnterpriseQuoteOrderShareCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
