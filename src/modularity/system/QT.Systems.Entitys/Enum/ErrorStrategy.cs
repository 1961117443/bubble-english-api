using QT.DependencyInjection;
using System.ComponentModel;

namespace QT.Systems.Entitys.Enum;

/// <summary>
/// 错误策略.
/// </summary>
[SuppressSniffer]
public enum ErrorStrategy
{
    /// <summary>
    /// 账号锁定.
    /// </summary>
    [Description("账号锁定")]
    Lock = 1,

    /// <summary>
    /// 延时登录.
    /// </summary>
    [Description("延时登录")]
    Delay = 2
}