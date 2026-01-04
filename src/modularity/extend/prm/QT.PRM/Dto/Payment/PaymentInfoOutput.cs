using QT.DependencyInjection;

namespace QT.PRM.Dto.Payment;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class PaymentInfoOutput: PaymentCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
