using QT.Common.Security;
using QT.DependencyInjection;

namespace QT.JZRC.Entitys.Dto.JzrcStoreroom;

/// <summary>
/// 功能下拉框输出.
/// </summary>
[SuppressSniffer]
public class JzrcStoreroomSelectorOutput : TreeModel
{
    /// <summary>
    /// 菜单名称.
    /// </summary>
    public string fullName { get; set; }
}