using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.QuoteOrder;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class QuoteOrderUpInput : QuoteOrderCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
