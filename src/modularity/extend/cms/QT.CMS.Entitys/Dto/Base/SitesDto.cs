using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Base;

/// <summary>
/// 站点(显示)
/// </summary>
public class SitesDto : SitesEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public int id { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(128)]
    public string? addBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;
}

/// <summary>
/// 站点(编辑)
/// </summary>
public class SitesEditDto
{
    /// <summary>
    /// 站点名称
    /// </summary>
    [Display(Name = "站点名称")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? name { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? title { get; set; }

    /// <summary>
    /// 模板目录名
    /// </summary>
    [Display(Name = "模板目录名")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? dirPath { get; set; }

    /// <summary>
    /// 默认站点
    /// </summary>
    [Display(Name = "默认站点")]
    public byte isDefault { get; set; } = 0;

    /// <summary>
    /// 网站LOGO
    /// </summary>
    [Display(Name = "网站LOGO")]
    [StringLength(512)]
    public string? logo { get; set; }

    /// <summary>
    /// 单位名称
    /// </summary>
    [Display(Name = "单位名称")]
    [StringLength(128)]
    public string? company { get; set; }

    /// <summary>
    /// 单位地址
    /// </summary>
    [Display(Name = "单位地址")]
    [StringLength(512)]
    public string? address { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    [Display(Name = "联系电话")]
    [StringLength(128)]
    public string? tel { get; set; }

    /// <summary>
    /// 传真号码
    /// </summary>
    [Display(Name = "传真号码")]
    [StringLength(128)]
    public string? fax { get; set; }

    /// <summary>
    /// 电子邮箱
    /// </summary>
    [Display(Name = "电子邮箱")]
    [StringLength(128)]
    public string? email { get; set; }

    /// <summary>
    /// 备案号
    /// </summary>
    [Display(Name = "备案号")]
    [StringLength(128)]
    public string? crod { get; set; }

    /// <summary>
    /// 版权信息
    /// </summary>
    [Display(Name = "版权信息")]
    [StringLength(1024)]
    public string? copyright { get; set; }

    /// <summary>
    /// SEO标题
    /// </summary>
    [Display(Name = "SEO标题")]
    [StringLength(512)]
    public string? seoTitle { get; set; }

    /// <summary>
    /// SEO关健字
    /// </summary>
    [Display(Name = "SEO关健字")]
    [StringLength(512)]
    public string? seoKeyword { get; set; }

    /// <summary>
    /// SEO描述
    /// </summary>
    [Display(Name = "SEO描述")]
    [StringLength(512)]
    public string? seoDescription { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;

    /// <summary>
    /// 状态0正常1禁用
    /// </summary>
    [Display(Name = "状态")]
    public byte status { get; set; } = 0;

    /// <summary>
    /// 域名列表
    /// </summary>
    public ICollection<SiteDomainDto>? domains { get; set; }
}
