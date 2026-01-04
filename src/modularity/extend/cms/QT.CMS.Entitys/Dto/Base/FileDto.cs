using System;
using System.Collections.Generic;
using System.Text;

namespace QT.CMS.Entitys.Dto.Base;

/// <summary>
/// 文件上传返回的实体
/// </summary>
public class FileDto
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string? fileName { get; set; }
    /// <summary>
    /// 文件路径
    /// </summary>
    public string? filePath { get; set; }
    /// <summary>
    /// 缩略图路径
    /// </summary>
    public List<string>? thumbPath { get; set; }
    /// <summary>
    /// 文件大小
    /// </summary>
    public long fileSize { get; set; }
    /// <summary>
    /// 文件扩展名
    /// </summary>
    public string? fileExt { get; set; }
}
