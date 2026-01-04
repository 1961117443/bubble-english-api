using QT.DependencyInjection;

namespace QT.PRM.Dto.Resident;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class ResidentUpInput : ResidentCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
