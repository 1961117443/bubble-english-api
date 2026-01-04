using QT.DependencyInjection;

namespace QT.PRM.Dto.Building;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class BuildingListOutput: BuildingInfoOutput
{
    /// <summary>
    /// 苑区名称
    /// </summary>
    public string communityIdName { get; set; }
}
