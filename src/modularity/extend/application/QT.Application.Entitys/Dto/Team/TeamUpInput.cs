using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.Team;

/// <summary>
/// 修改分组输入.
/// </summary>
[SuppressSniffer]
public class TeamUpInput : TeamCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}