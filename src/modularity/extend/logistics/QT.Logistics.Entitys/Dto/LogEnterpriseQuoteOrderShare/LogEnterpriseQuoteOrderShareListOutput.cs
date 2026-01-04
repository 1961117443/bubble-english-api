using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrderShare;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class LogEnterpriseQuoteOrderShareListOutput: LogEnterpriseQuoteOrderShareInfoOutput
{
    public string orderNo { get; set; }
}
