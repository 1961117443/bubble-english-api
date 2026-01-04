using QT.DependencyInjection;
using System.ComponentModel;

namespace QT.Common.Enum;

/// <summary>
/// 文件预览方式.
/// </summary>
[SuppressSniffer]
public enum PreviewType
{
    /// <summary>
    /// KKfile.
    /// </summary>
    [Description("KKfile")]
    kkfile = 0,

    /// <summary>
    /// 永中.
    /// </summary>
    [Description("yozo")]
    yozo = 1,
}