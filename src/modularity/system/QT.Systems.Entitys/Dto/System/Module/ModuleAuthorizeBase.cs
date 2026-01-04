using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.Module;

/// <summary>
/// 功能按钮输出.
/// </summary>
[SuppressSniffer]
public class ModuleAuthorizeBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 模块主键.
    /// </summary>
    public string moduleId { get; set; }
}