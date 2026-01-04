using QT.DependencyInjection;
using System.ComponentModel;

namespace QT.Common.Enum;

/// <summary>
/// 登录方式.
/// </summary>
[SuppressSniffer]
public enum LoginMethod
{
    /// <summary>
    /// 单一登录.
    /// </summary>
    [Description("单一登录")]
    Single = 1,

    /// <summary>
    /// 同时登录.
    /// </summary>
    [Description("同时登录")]
    SameTime = 2
}