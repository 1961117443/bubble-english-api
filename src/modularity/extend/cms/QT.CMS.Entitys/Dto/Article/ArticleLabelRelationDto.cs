using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Article;

/// <summary>
/// 文章标签关联
/// </summary>
public class ArticleLabelRelationDto
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
    /// 所属标签ID
    /// </summary>
    [Display(Name = "所属标签")]
    public long labelId { get; set; }

}