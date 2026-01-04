using QT.DependencyInjection;

namespace QT.PRM.Dto.Room;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class RoomUpInput : RoomCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
