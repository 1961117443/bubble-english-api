using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Article;

/// <summary>
/// 文章投稿扩展字段(显示)
/// </summary>
public class ArticleContributeFieldValueDto
{
    /// <summary>
    /// 所属字段ID
    /// </summary>
    [Display(Name = "所属字段")]
    public long fieldId { get; set; }

    /// <summary>
    /// 字段名
    /// </summary>
    [Display(Name = "字段名")]
    [StringLength(128)]
    public string? fieldName { get; set; }

    /// <summary>
    /// 字段值
    /// </summary>
    [Display(Name = "字段值")]
    public string? fieldValue { get; set; }

}