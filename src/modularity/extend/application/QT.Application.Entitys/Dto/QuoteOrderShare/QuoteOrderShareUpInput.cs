using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.QuoteOrderShare;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class QuoteOrderShareUpInput : QuoteOrderShareCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
