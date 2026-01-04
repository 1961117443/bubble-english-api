using QT.DependencyInjection;

namespace QT.PRM.Dto.RoomFee;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class RoomFeeInfoOutput: RoomFeeCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
