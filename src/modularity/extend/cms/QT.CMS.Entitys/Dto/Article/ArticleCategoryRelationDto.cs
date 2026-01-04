using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Article;

/// <summary>
/// 分类关系DTO
/// </summary>
public class ArticleCategoryRelationDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 所属文章ID
    /// </summary>
    [Display(Name = "所属文章")]
    public long articleId { get; set; }

    /// <summary>
    /// 所属分类
    /// </summary>
    [Display(Name = "所属分类")]
    public long categoryId { get; set; }

}