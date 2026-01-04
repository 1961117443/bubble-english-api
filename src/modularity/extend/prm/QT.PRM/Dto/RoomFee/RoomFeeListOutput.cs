using QT.DependencyInjection;

namespace QT.PRM.Dto.RoomFee;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class RoomFeeListOutput: RoomFeeInfoOutput
{

    public string roomName { get; set; }

    public string feeItemIdName { get; set; }
}
