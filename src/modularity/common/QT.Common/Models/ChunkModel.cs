using QT.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace QT.Common.Models;

/// <summary>
/// 文件分片模型
/// </summary>
[SuppressSniffer]
public class ChunkModel
{
    /// <summary>
    /// 当前文件块，从1开始.
    /// </summary>
    public int chunkNumber { get; set; }

    /// <summary>
    /// 当前分块大小.
    /// </summary>
    public int currentChunkSize { get; set; }

    /// <summary>
    /// 分块大小.
    /// </summary>
    public long chunkSize { get; set; }

    /// <summary>
    /// 总大小.
    /// </summary>
    public long totalSize { get; set; }

    /// <summary>
    /// 文件标识.
    /// </summary>
    public string identifier { get; set; }

    /// <summary>
    /// 文件名.
    /// </summary>
    public string fileName { get; set; }

    /// <summary>
    /// 相对路径.
    /// </summary>
    public string relativePath { get; set; }

    /// <summary>
    /// 总块数.
    /// </summary>
    public int totalChunks { get; set; }

    /// <summary>
    /// 文件存储类型.
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// 文件后缀.
    /// </summary>
    public string extension { get; set; }

    /// <summary>
    /// 文件类型.
    /// </summary>
    public string fileType { get; set; }

    /// <summary>
    /// 上级id.
    /// </summary>
    public string parentId { get; set; }

    /// <summary>
    /// 文件大小.
    /// </summary>
    public string fileSize { get; set; }

    /// <summary>
    /// 是否生成文件名.
    /// </summary>
    public bool isUpdateName { get; set; } = true;

    /// <summary>
    /// 文件.
    /// </summary>
    public IFormFile file { get; set; }
}
