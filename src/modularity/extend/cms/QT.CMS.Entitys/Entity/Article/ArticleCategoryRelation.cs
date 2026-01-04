using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 文章类别关联
/// </summary>
[SugarTable("cms_article_category_relation")]
[Tenant(ClaimConst.TENANTID)]
public class ArticleCategoryRelation
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属文章
    /// </summary>
    [Display(Name = "所属文章")]
    [ForeignKey("Article")]
    public long ArticleId { get; set; }

    /// <summary>
    /// 所属分类
    /// </summary>
    [Display(Name = "所属分类")]
    [ForeignKey("Category")]
    public long CategoryId { get; set; }

    /// <summary>
    /// 文章信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ArticleId))]
    public Articles? Article { get; set; }

    /// <summary>
    /// 类别信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(CategoryId))] 
    public ArticleCategory? Category { get; set; }
}
