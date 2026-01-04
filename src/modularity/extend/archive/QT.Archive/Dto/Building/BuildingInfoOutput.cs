using QT.DependencyInjection;

namespace QT.Archive.Dto.Building;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class BuildingInfoOutput: BuildingCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
