using QT.DependencyInjection;

namespace QT.PRM.Dto.Room;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class RoomListOutput: RoomInfoOutput
{
    /// <summary>
    /// 苑区/楼栋
    /// </summary>
    public string communityBuilding { get; set; }
}
