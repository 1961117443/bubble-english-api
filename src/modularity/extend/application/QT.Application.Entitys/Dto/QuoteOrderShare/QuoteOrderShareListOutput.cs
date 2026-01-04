using QT.DependencyInjection;
using QT.Extend.Entitys.Dto.ProjectGantt;

namespace QT.Extend.Entitys.Dto.QuoteOrderShare;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class QuoteOrderShareListOutput: QuoteOrderShareInfoOutput
{
    public string orderNo { get; set; }
}
