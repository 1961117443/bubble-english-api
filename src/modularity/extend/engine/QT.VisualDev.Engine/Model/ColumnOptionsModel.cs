using QT.DependencyInjection;

namespace QT.VisualDev.Engine;

/// <summary>
/// 列配置模型.
/// </summary>
[SuppressSniffer]
public class ColumnOptionsModel
{
    /// <summary>
    /// 字段.
    /// </summary>
    public string value { get; set; }

    /// <summary>
    /// 列名.
    /// </summary>
    public string label { get; set; }
}