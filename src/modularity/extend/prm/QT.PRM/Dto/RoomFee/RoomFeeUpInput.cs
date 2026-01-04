using QT.DependencyInjection;

namespace QT.PRM.Dto.RoomFee;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class RoomFeeUpInput : RoomFeeCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
