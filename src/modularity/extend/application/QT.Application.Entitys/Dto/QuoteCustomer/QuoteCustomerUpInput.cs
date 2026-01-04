using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.QuoteCustomer;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class QuoteCustomerUpInput : QuoteCustomerCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
