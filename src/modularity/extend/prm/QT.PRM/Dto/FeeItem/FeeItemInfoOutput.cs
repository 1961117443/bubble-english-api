using QT.DependencyInjection;

namespace QT.PRM.Dto.FeeItem;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class FeeItemInfoOutput: FeeItemCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
