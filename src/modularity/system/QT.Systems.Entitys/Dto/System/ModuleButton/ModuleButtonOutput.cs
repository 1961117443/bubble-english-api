using QT.DependencyInjection;
using QT.Systems.Entitys.Dto.Module;

namespace QT.Systems.Entitys.Dto.ModuleButton;

/// <summary>
/// 功能按钮输出.
/// </summary>
[SuppressSniffer]
public class ModuleButtonOutput : ModuleAuthorizeBase
{
    /// <summary>
    /// 按钮编码.
    /// </summary>
    public string enCode { get; set; }
}