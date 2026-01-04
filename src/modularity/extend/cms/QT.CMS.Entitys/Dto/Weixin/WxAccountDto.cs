using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Weixin;
/// <summary>
/// 公众号(显示)
/// </summary>
public class WxAccountDto : WxAccountEditDto
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

    /// <summary>
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    [StringLength(128)]
    public string? updateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? updateTime { get; set; }
}

/// <summary>
/// 公众号(编辑)
/// </summary>
public class WxAccountEditDto
{
    /// <summary>
    /// 站点ID
    /// </summary>
    [Display(Name = "站点ID")]
    public int siteId { get; set; }

    /// <summary>
    /// 公众号名称
    /// </summary>
    [Display(Name = "公众号名称")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(128, ErrorMessage = "{0}最多{1}位字符")]
    public string? title { get; set; }

    /// <summary>
    /// AppId
    /// </summary>
    [Display(Name = "AppId")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(128, ErrorMessage = "{0}最多{1}位字符")]
    public string? appId { get; set; }

    /// <summary>
    /// AppSecret
    /// </summary>
    [Display(Name = "AppSecret")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(128, ErrorMessage = "{0}最多{1}位字符")]
    public string? appSecret { get; set; }

    /// <summary>
    /// Token
    /// </summary>
    [Display(Name = "Token")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(128, ErrorMessage = "{0}最多{1}位字符")]
    public string? token { get; set; }

    /// <summary>
    /// EncodingAESKey
    /// </summary>
    [Display(Name = "EncodingAESKey")]
    [StringLength(128)]
    public string? encodingAESKey { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;

    /// <summary>
    /// 内容推送0关闭1开启
    /// </summary>
    [Display(Name = "内容推送")]
    public byte status { get; set; } = 0;
}
