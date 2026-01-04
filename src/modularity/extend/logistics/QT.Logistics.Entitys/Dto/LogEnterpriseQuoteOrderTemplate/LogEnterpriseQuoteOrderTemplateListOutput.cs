using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrderTemplate;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class LogEnterpriseQuoteOrderTemplateListOutput: LogEnterpriseQuoteOrderTemplateInfoOutput
{
    public DateTime? creatorTime { get; set; }
}
