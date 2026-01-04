using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 文章标签关联
/// </summary>
[SugarTable("cms_article_label_relation")]
[Tenant(ClaimConst.TENANTID)]
public class ArticleLabelRelation
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属文章ID
    /// </summary>
    [Display(Name = "所属文章")]
    [ForeignKey("Article")]
    public long ArticleId { get; set; }

    /// <summary>
    /// 所属标签
    /// </summary>
    [Display(Name = "所属标签")]
    [ForeignKey("ArticleLabel")]
    public long LabelId { get; set; }

    /// <summary>
    /// 文章信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ArticleId))]
    public Articles? Article { get; set; }

    /// <summary>
    /// 标签信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(LabelId))]
    public ArticleLabel? Label { get; set; }
}
