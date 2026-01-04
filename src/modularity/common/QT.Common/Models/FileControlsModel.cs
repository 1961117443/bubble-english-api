using QT.DependencyInjection;

namespace QT.Common.Models;

/// <summary>
/// 文件控件模型.
/// </summary>
[SuppressSniffer]
public class FileControlsModel
{
    /// <summary>
    /// 文件名称.
    /// </summary>
    public string? name { get; set; }

    /// <summary>
    /// 文件ID.
    /// </summary>
    public string? fileId { get; set; }

    /// <summary>
    /// 下载地址.
    /// </summary>
    public string? url { get; set; }
}