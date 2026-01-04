using QT.DependencyInjection;

namespace QT.Systems.Entitys.Model.PrintDev;

/// <summary>
/// 打印模板配置字段模型.
/// </summary>
[SuppressSniffer]
public class PrintDevFieldModel
{
    /// <summary>
    /// 字段名.
    /// </summary>
    public string field { get; set; }

    /// <summary>
    /// 字段说明.
    /// </summary>
    public string fieldName { get; set; }
}