using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.QuoteOrderTemplate;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class QuoteOrderTemplateUpInput : QuoteOrderTemplateCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
