using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.ModuleColumn;

/// <summary>
/// 功能列修改输入.
/// </summary>
[SuppressSniffer]
public class ModuleColumnUpInput : ModuleColumnCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}