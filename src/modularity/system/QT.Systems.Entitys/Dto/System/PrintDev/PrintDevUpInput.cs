using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.PrintDev;

/// <summary>
/// 打印模板配置修复输入.
/// </summary>
[SuppressSniffer]
public class PrintDevUpInput : PrintDevCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}