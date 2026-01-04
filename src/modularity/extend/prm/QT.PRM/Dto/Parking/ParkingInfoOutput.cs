using QT.DependencyInjection;

namespace QT.PRM.Dto.Parking;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class ParkingInfoOutput: ParkingCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
