using QT.DependencyInjection;

namespace QT.PRM.Dto.Parking;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class ParkingListOutput: ParkingInfoOutput
{
    public string roomName { get; set; }
}
