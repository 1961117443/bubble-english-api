using QT.Common.Contracts;
using SqlSugar;

namespace QT.Archive.Entity;

/// <summary>
/// 档案扫描件信息
/// </summary>
[SugarTable("ext_archives_document")]
public class ArchivesDocumentEntity : CLDEntityBase
{

    /// <summary>
    /// 档案id
    /// </summary>
    [SugarColumn(ColumnName = "F_Aid")]
    public string Aid { get; set; }

    /// <summary>
    /// 文档分类(分辨率) 默认是0
    /// </summary>
    [SugarColumn(ColumnName = "F_Type")]
    public int Type { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    [SugarColumn(ColumnName = "F_FullName")]
    public string FullName { get; set; }


    /// <summary>
    /// 文件路径
    /// </summary>
    [SugarColumn(ColumnName = "F_FilePath")]
    public string FilePath { get; set; }


    /// <summary>
    /// 文件大小
    /// </summary>
    [SugarColumn(ColumnName = "F_FileSize")]
    public string FileSize { get; set; }



    /// <summary>
    /// 文件后缀
    /// </summary>
    [SugarColumn(ColumnName = "F_FileExtension")]
    public string FileExtension { get; set; }
}