namespace QT.Archive.Dto.Archives;

public class ArchivesDocumentInfo
{
    public string id { get; set; }

    /// <summary>
    /// 档案id
    /// </summary>
    public string aid { get; set; }

    /// <summary>
    /// 文档分类(分辨率)
    /// </summary>
    public int type { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 文件路径
    /// </summary>
    public string filePath { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public string fileSize { get; set; }

    /// <summary>
    /// 文件后缀
    /// </summary>
    public string fileExtension { get; set; }
}