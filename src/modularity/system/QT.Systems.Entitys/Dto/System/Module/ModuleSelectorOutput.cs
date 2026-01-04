using QT.Common.Security;
using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.Module;

/// <summary>
/// 功能下拉框输出.
/// </summary>
[SuppressSniffer]
public class ModuleSelectorOutput : TreeModel
{
    /// <summary>
    /// 菜单名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 图标.
    /// </summary>
    public string icon { get; set; }
}