using QT.DependencyInjection;

namespace QT.PRM.Dto.Building;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class BuildingUpInput : BuildingCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
