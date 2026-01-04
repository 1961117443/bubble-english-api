using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 文章扩展字段值
/// </summary>
[SugarTable("cms_article_field_value")]
[Tenant(ClaimConst.TENANTID)]
public class ArticleFieldValue
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
    /// 所属字段ID
    /// </summary>
    [Display(Name = "所属字段")]
    public long FieldId { get; set; }

    /// <summary>
    /// 字段名
    /// </summary>
    [Display(Name = "字段名")]
    [StringLength(128)]
    public string? FieldName { get; set; }

    /// <summary>
    /// 字段值
    /// </summary>
    [Display(Name = "字段值")]
    public string? FieldValue { get; set; }

    /// <summary>
    /// 文章信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ArticleId))]
    public Articles? Article { get; set; }
}
