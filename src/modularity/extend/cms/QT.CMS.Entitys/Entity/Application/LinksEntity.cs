using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 友情链接
/// </summary>
[SugarTable("cms_app_links")]
[Tenant(ClaimConst.TENANTID)]
public class LinksEntity
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true)]
    public int Id { get; set; }

    /// <summary>
    /// 站点ID
    /// </summary>
    [Display(Name = "站点ID")]
    public int SiteId { get; set; }

    /// <summary>
    /// 网站标题
    /// </summary>
    [Display(Name = "网站标题")]
    [Required]
    [StringLength(128)]
    public string? Title { get; set; } = string.Empty;

    /// <summary>
    /// 联系电话
    /// </summary>
    [Display(Name = "联系电话")]
    [Required]
    [StringLength(30)]
    public string? TelPhone { get; set; } = string.Empty;

    /// <summary>
    /// 网址
    /// </summary>
    [Display(Name = "网址")]
    [Required]
    [StringLength(250)]
    public string? SiteUrl { get; set; } = string.Empty;


    /// <summary>
    /// 网站LOGO
    /// </summary>
    [Display(Name = "网站LOGO")]
    [StringLength(512)]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int SortId { get; set; } = 99;

    /// <summary>
    /// 是否推荐0不推荐1推荐
    /// </summary>
    [Display(Name = "是否推荐")]
    public byte IsRecom { get; set; } = 0;

    /// <summary>
    /// 状态0未审核1正常
    /// </summary>
    [Display(Name = "状态")]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;
}
