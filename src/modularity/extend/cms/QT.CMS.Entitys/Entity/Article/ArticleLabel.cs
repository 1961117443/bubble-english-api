using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 文章标签
/// </summary>
[SugarTable("cms_article_label")]
[Tenant(ClaimConst.TENANTID)]
public class ArticleLabel
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true,IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [StringLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int SortId { get; set; } = 99;

    /// <summary>
    /// 状态0正常1禁用
    /// </summary>
    [Display(Name = "状态")]
    [Range(0, 9)]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(30)]
    public string? AddBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    [StringLength(30)]
    public string? UpdateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 标签关联列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ArticleLabelRelation.LabelId))]
    public ICollection<ArticleLabelRelation> LabelRelations { get; set; } = new List<ArticleLabelRelation>();
}
