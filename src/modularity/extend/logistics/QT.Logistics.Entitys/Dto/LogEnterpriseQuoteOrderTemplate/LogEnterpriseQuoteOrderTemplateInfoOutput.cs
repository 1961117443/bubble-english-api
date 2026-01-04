using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrderTemplate;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class LogEnterpriseQuoteOrderTemplateInfoOutput: LogEnterpriseQuoteOrderTemplateCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
