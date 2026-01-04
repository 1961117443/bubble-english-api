using System.ComponentModel.DataAnnotations;
namespace QT.CMS.Entitys.Dto.Article;

/// <summary>
/// 文章扩展字段值(显示)
/// </summary>
public class ArticleFieldValueDto : ArticleFieldValueEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }
}

/// <summary>
/// 文章扩展字段值(编辑)
/// </summary>
public class ArticleFieldValueEditDto
{
    /// <summary>
    /// 所属文章
    /// </summary>
    [Display(Name = "所属文章")]
    public long articleId { get; set; }

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