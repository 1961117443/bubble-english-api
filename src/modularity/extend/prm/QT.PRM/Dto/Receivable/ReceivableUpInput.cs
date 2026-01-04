using QT.DependencyInjection;

namespace QT.PRM.Dto.Receivable;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class ReceivableUpInput : ReceivableCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
