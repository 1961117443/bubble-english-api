using QT.DependencyInjection;

namespace QT.WorkFlow.Entitys.Model.Item;

[SuppressSniffer]
public class ConditionsItem
{
    /// <summary>
    /// 字段名称.
    /// </summary>
    public string? fieldName { get; set; }

    /// <summary>
    /// 比较名称.
    /// </summary>
    public string? symbolName { get; set; }

    /// <summary>
    /// 字段值.
    /// </summary>
    public dynamic filedValue { get; set; }

    /// <summary>
    /// 逻辑名称.
    /// </summary>
    public string? logicName { get; set; }

    /// <summary>
    /// 字段.
    /// </summary>
    public string? field { get; set; }

    /// <summary>
    /// 逻辑符号.
    /// </summary>
    public string? logic { get; set; }

    /// <summary>
    /// 比较符号.
    /// </summary>
    public string? symbol { get; set; }

    /// <summary>
    /// 控件类型.
    /// </summary>
    public string? qtKey { get; set; }
}
