using QT.DependencyInjection;

namespace QT.PRM.Dto.Community;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class CommunityInfoOutput: CommunityCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
