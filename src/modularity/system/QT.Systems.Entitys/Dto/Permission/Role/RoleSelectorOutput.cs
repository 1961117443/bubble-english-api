using QT.Common.Security;
using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.Role;

/// <summary>
/// 角色下拉输出.
/// </summary>
[SuppressSniffer]
public class RoleSelectorOutput : TreeModel
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 类型.
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// 编码.
    /// </summary>
    public string enCode { get; set; }
}