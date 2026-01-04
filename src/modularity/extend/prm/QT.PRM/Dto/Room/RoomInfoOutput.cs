using QT.DependencyInjection;

namespace QT.PRM.Dto.Room;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class RoomInfoOutput: RoomCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
