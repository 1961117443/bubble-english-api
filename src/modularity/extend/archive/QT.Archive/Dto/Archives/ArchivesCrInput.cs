using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Archive.Dto.Archives;

/// <summary>
/// 楼栋创建输入
/// </summary>
[SuppressSniffer]
public class ArchivesCrInput
{
    /// <summary>
    /// 档案位置
    /// </summary>
    public string bid { get; set; }

    /// <summary>
    /// 编号
    /// </summary>
    [Required]
    public string code { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Required]
    public string name { get; set; }

    /// <summary>
    /// 建立日期
    /// </summary>
    public DateTime? establishmentDate { get; set; }

    /// <summary>
    /// 销毁日期
    /// </summary>
    public DateTime? destructionDate { get; set; }

    /// <summary>
    /// 文档集合
    /// </summary>
    public List<ArchivesDocumentInfo> documentList { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string label { get; set; }
}
