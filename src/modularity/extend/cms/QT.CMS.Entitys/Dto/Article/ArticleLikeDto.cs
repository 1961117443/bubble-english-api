using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Article;

/// <summary>
/// 文章点赞(公共)
/// </summary>
public class ArticleLikeBaseDto
{
    /// <summary>
    /// 所属文章ID
    /// </summary>
    [Display(Name = "所属文章")]
    public long articleId { get; set; }
}

/// <summary>
/// 文章点赞(List)
/// </summary>
public class ArticleLikeDto: ArticleLikeBaseDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 所属用户
    /// </summary>
    [Display(Name = "所属用户")]
    public int userId { get; set; }

    /// <summary>
    /// 点赞时间
    /// </summary>
    [Display(Name = "点赞时间")]
    public DateTime addTime { get; set; } = DateTime.Now;
}

/// <summary>
/// 文章点赞(add)
/// </summary>
public class ArticleLikeAddDto: ArticleLikeBaseDto
{
    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    public int userId { get; set; }

    /// <summary>
    /// 点赞时间
    /// </summary>
    [Display(Name = "点赞时间")]
    public DateTime addTime { get; set; } = DateTime.Now;
}

/// <summary>
/// 文章点赞(Edit)
/// </summary>
public class ArticleLikeEditDto: ArticleLikeBaseDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }
}
