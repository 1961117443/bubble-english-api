using QT.DependencyInjection;
using System.ComponentModel;

namespace QT.Systems.Entitys.Enum;

/// <summary>
/// 菜单分类.
/// </summary>
[SuppressSniffer]
public enum MenuCategory
{
    /// <summary>
    /// Web端.
    /// </summary>
    [Description("Web端")]
    Web,

    /// <summary>
    /// App端.
    /// </summary>
    [Description("App端")]
    App
}