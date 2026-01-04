using QT.DependencyInjection;

namespace QT.VisualDev.Engine;

/// <summary>
/// 插槽模型.
/// </summary>
[SuppressSniffer]
public class SlotModel
{
    /// <summary>
    /// 前.
    /// </summary>
    public string prepend { get; set; }

    /// <summary>
    /// 后.
    /// </summary>
    public string append { get; set; }

    /// <summary>
    /// 默认名称.
    /// </summary>
    public string defaultName { get; set; }

    /// <summary>
    /// 配置项.
    /// </summary>
    public List<Dictionary<string, object>> options { get; set; }

    /// <summary>
    /// app配置项.
    /// </summary>
    public string appOptions { get; set; }

    /// <summary>
    /// 默认.
    /// </summary>
    public string @default { get; set; }
}