using QT.DependencyInjection;

namespace QT.VisualDev.Engine;

/// <summary>
/// 按钮配置模型.
/// </summary>
[SuppressSniffer]
public class ButtonConfigModel
{
    /// <summary>
    /// 值.
    /// </summary>
    public string value { get; set; }

    /// <summary>
    /// 图标.
    /// </summary>
    public string icon { get; set; }

    /// <summary>
    /// 标签.
    /// </summary>
    public string label { get; set; }
}