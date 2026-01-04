using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.QuoteCustomer;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class QuoteCustomerInfoOutput: QuoteCustomerCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
