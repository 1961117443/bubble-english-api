using QT.DependencyInjection;
using System.ComponentModel;

namespace QT.Common.Enum;

/// <summary>
/// 查询类型的枚举.
/// </summary>
[SuppressSniffer]
public enum QueryType
{
    /// <summary>
    /// 等于.
    /// </summary>
    [Description("等于")]
    Equal,

    /// <summary>
    /// 模糊.
    /// </summary>
    [Description("模糊")]
    Contains,

    /// <summary>
    /// 大于.
    /// </summary>
    [Description("大于")]
    GreaterThan,

    /// <summary>
    /// 小于.
    /// </summary>
    [Description("小于")]
    LessThan,

    /// <summary>
    /// 不等于.
    /// </summary>
    [Description("不等于")]
    NotEqual,

    /// <summary>
    /// 大于等于.
    /// </summary>
    [Description("大于等于")]
    GreaterThanOrEqual,

    /// <summary>
    /// 小于等于.
    /// </summary>
    [Description("小于等于")]
    LessThanOrEqual,

    /// <summary>
    /// 包含.
    /// </summary>
    [Description("包含")]
    In
}