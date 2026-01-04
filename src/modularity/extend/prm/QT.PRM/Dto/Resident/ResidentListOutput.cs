using QT.DependencyInjection;

namespace QT.PRM.Dto.Resident;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class ResidentListOutput: ResidentInfoOutput
{
    public string roomName { get; set; }
}
