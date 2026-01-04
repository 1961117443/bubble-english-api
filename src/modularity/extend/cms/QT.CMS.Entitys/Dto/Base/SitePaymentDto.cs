using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Base;

/// <summary>
/// 站点支付方式(显示)
/// </summary>
public class SitePaymentDto: SitePaymentEditDto
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
    /// 站点信息
    /// </summary>
    public SitesDto? site { get; set; }

    /// <summary>
    /// 支付方式信息
    /// </summary>
    public PaymentsDto? payment { get; set; }
}

/// <summary>
/// 站点支付方式(编辑)
/// </summary>
public class SitePaymentEditDto
{
    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int siteId { get; set; }

    /// <summary>
    /// 支付方式ID
    /// </summary>
    [Display(Name = "支付方式")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int paymentId { get; set; }

    /// <summary>
    /// 接口类型
    /// 线下付款：cash
    /// 余额支付：balance
    /// 支付宝：pc(电脑)|h5(手机)|app
    /// 微信：native(扫码)|h5(手机)|mp(小程序)|jsapi(公众号)|app
    /// </summary>
    [Display(Name = "接口类型")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? type { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? title { get; set; }

    /// <summary>
    /// 通讯密钥1
    /// </summary>
    [Display(Name = "通讯密钥1")]
    public string? key1 { get; set; }

    /// <summary>
    /// 通讯密钥2
    /// </summary>
    [Display(Name = "通讯密钥2")]
    public string? key2 { get; set; }

    /// <summary>
    /// 通讯密钥3
    /// </summary>
    [Display(Name = "通讯密钥3")]
    public string? key3 { get; set; }

    /// <summary>
    /// 通讯密钥4
    /// </summary>
    [Display(Name = "通讯密钥4")]
    public string? key4 { get; set; }

    /// <summary>
    /// 通讯密钥5
    /// </summary>
    [Display(Name = "通讯密钥5")]
    public string? key5 { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;
}
