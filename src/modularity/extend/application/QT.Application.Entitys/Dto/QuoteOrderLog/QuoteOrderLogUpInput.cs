using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.QuoteOrderLog;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class QuoteOrderLogUpInput : QuoteOrderLogCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
