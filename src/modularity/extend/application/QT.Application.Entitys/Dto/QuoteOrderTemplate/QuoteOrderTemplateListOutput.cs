using QT.DependencyInjection;
using QT.Extend.Entitys.Dto.ProjectGantt;

namespace QT.Extend.Entitys.Dto.QuoteOrderTemplate;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class QuoteOrderTemplateListOutput: QuoteOrderTemplateInfoOutput
{
    public DateTime? creatorTime { get; set; }
}
