using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.Organize;

/// <summary>
/// 修改机构输入.
/// </summary>
[SuppressSniffer]
public class OrganizeUpInput : OrganizeCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}