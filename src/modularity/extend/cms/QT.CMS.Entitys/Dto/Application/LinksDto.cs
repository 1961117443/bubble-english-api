using QT.CMS.Entitys.Dto.Login;
using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Application;

/// <summary>
/// 友情链接(显示)
/// </summary>
public class LinksDto: LinksEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public int id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;
}

/// <summary>
/// 友情链接(编辑)
/// </summary>
public class LinksEditDto
{
    /// <summary>
    /// 站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int siteId { get; set; }

    /// <summary>
    /// 网站标题
    /// </summary>
    [Display(Name = "网站标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(128, ErrorMessage = "{0}不可超出{1}字符")]
    public string? title { get; set; } = string.Empty;

    /// <summary>
    /// 联系电话
    /// </summary>
    [Display(Name = "联系电话")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(30, ErrorMessage = "{0}不可超出{1}字符")]
    public string? telPhone { get; set; } = string.Empty;

    /// <summary>
    /// 网址
    /// </summary>
    [Display(Name = "网址")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(250, ErrorMessage = "{0}不可超出{1}字符")]
    public string? siteUrl { get; set; } = string.Empty;


    /// <summary>
    /// 网站LOGO
    /// </summary>
    [Display(Name = "网站LOGO")]
    [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
    public string? logoUrl { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int sortId { get; set; } = 99;

    /// <summary>
    /// 是否推荐0不推荐1推荐
    /// </summary>
    [Display(Name = "是否推荐")]
    public byte isRecom { get; set; } = 0;

    /// <summary>
    /// 状态0未审核1正常
    /// </summary>
    [Display(Name = "状态")]
    public byte status { get; set; } = 0;
}

/// <summary>
/// 友情链接(前端)
/// </summary>
public class LinksClientDto : VerifyCode
{
    /// <summary>
    /// 站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int siteId { get; set; }

    /// <summary>
    /// 网站标题
    /// </summary>
    [Display(Name = "网站标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(128, ErrorMessage = "{0}不可超出{1}字符")]
    public string? title { get; set; } = string.Empty;

    /// <summary>
    /// 联系电话
    /// </summary>
    [Display(Name = "联系电话")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(30, ErrorMessage = "{0}不可超出{1}字符")]
    public string? telPhone { get; set; } = string.Empty;

    /// <summary>
    /// 网址
    /// </summary>
    [Display(Name = "网址")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(250, ErrorMessage = "{0}不可超出{1}字符")]
    public string? siteUrl { get; set; } = string.Empty;


    /// <summary>
    /// 网站LOGO
    /// </summary>
    [Display(Name = "网站LOGO")]
    [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
    public string? logoUrl { get; set; } = string.Empty;
}
