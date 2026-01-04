using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

[SugarTable("cms_wx_account")]
public class WxAccount
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
    /// 公众号名称
    /// </summary>
    [Display(Name = "公众号名称")]
    [Required]
    [StringLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// AppId
    /// </summary>
    [Display(Name = "AppId")]
    [Required]
    [StringLength(128)]
    public string? AppId { get; set; }

    /// <summary>
    /// AppSecret
    /// </summary>
    [Display(Name = "AppSecret")]
    [Required]
    [StringLength(128)]
    public string? AppSecret { get; set; }

    /// <summary>
    /// Token
    /// </summary>
    [Display(Name = "Token")]
    [Required]
    [StringLength(128)]
    public string? Token { get; set; }

    /// <summary>
    /// EncodingAESKey
    /// </summary>
    [Display(Name = "EncodingAESKey")]
    [StringLength(128)]
    public string? EncodingAESKey { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int SortId { get; set; } = 99;

    /// <summary>
    /// 内容推送0关闭1开启
    /// </summary>
    [Display(Name = "内容推送")]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(128)]
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
    [StringLength(128)]
    public string? UpdateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? UpdateTime { get; set; }
}
