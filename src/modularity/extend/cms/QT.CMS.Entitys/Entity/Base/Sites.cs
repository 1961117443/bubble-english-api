using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 站点列表
/// </summary>
[SugarTable("cms_sites")]
[Tenant(ClaimConst.TENANTID)]
public class Sites
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name ="自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true,IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 站点英文名
    /// </summary>
    [Display(Name = "站点英文名")]
    [Required]
    [StringLength(128)]
    public string? Name { get; set; }

    /// <summary>
    /// 模板目录名
    /// </summary>
    [Display(Name = "模板目录名")]
    [Required]
    [StringLength(128)]
    public string? DirPath { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required]
    [StringLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// 网站LOGO
    /// </summary>
    [Display(Name = "网站LOGO")]
    [StringLength(512)]
    public string? Logo { get; set; }

    /// <summary>
    /// 单位名称
    /// </summary>
    [Display(Name = "单位名称")]
    [StringLength(128)]
    public string? Company { get; set; }

    /// <summary>
    /// 单位地址
    /// </summary>
    [Display(Name = "单位地址")]
    [StringLength(512)]
    public string? Address { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    [Display(Name = "联系电话")]
    [StringLength(30)]
    public string? Tel { get; set; }

    /// <summary>
    /// 传真号码
    /// </summary>
    [Display(Name = "传真号码")]
    [StringLength(30)]
    public string? Fax { get; set; }

    /// <summary>
    /// 电子邮箱
    /// </summary>
    [Display(Name = "电子邮箱")]
    [StringLength(60)]
    public string? Email { get; set; }

    /// <summary>
    /// 备案号
    /// </summary>
    [Display(Name = "备案号")]
    [StringLength(30)]
    public string? Crod { get; set; }

    /// <summary>
    /// 版权信息
    /// </summary>
    [Display(Name = "版权信息")]
    [StringLength(512)]
    public string? Copyright { get; set; }

    /// <summary>
    /// SEO标题
    /// </summary>
    [Display(Name = "SEO标题")]
    [StringLength(512)]
    public string? SeoTitle { get; set; }

    /// <summary>
    /// SEO关健字
    /// </summary>
    [Display(Name = "SEO关健字")]
    [StringLength(512)]
    public string? SeoKeyword { get; set; }

    /// <summary>
    /// SEO描述
    /// </summary>
    [Display(Name = "SEO描述")]
    [StringLength(512)]
    public string? SeoDescription { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int SortId { get; set; } = 99;

    /// <summary>
    /// 是否默认站
    /// </summary>
    [Display(Name = "是否默认站")]
    public byte IsDefault { get; set; } = 0;

    /// <summary>
    /// 状态0正常1禁用
    /// </summary>
    [Display(Name = "状态")]
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
    public DateTime? AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 域名列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(SiteDomain.SiteId))]
    public List<SiteDomain> Domains { get; set; }
}
