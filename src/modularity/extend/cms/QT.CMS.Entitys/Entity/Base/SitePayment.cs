using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 站点支付方式
/// </summary>
[SugarTable("cms_site_payment")]
[Tenant(ClaimConst.TENANTID)]
public class SitePayment
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 所属站点
    /// </summary>
    [Display(Name = "所属站点")]
    [ForeignKey("Site")]
    public int SiteId { get; set; }

    /// <summary>
    /// 支付方式
    /// </summary>
    [Display(Name = "支付方式")]
    [ForeignKey("Payment")]
    public int PaymentId { get; set; }

    /// <summary>
    /// 接口类型
    /// 线下付款：cash
    /// 余额支付：balance
    /// 支付宝：pc(电脑)|h5(手机)|app
    /// 微信：native(扫码)|h5(手机)|mp(小程序)|jsapi(公众号)|app
    /// </summary>
    [Display(Name = "接口类型")]
    [Required]
    [StringLength(30)]
    public string? Type { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required]
    [StringLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// 通讯密钥1
    /// </summary>
    [Display(Name = "通讯密钥1")]
    public string? Key1 { get; set; }

    /// <summary>
    /// 通讯密钥2
    /// </summary>
    [Display(Name = "通讯密钥2")]
    public string? Key2 { get; set; }

    /// <summary>
    /// 通讯密钥3
    /// </summary>
    [Display(Name = "通讯密钥3")]
    public string? Key3 { get; set; }

    /// <summary>
    /// 通讯密钥3
    /// </summary>
    [Display(Name = "通讯密钥4")]
    public string? Key4 { get; set; }

    /// <summary>
    /// 通讯密钥5
    /// </summary>
    [Display(Name = "通讯密钥5")]
    public string? Key5 { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    [StringLength(1024)]
    public int SortId { get; set; } = 99;

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
    /// 站点信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(SiteId))]
    public Sites? Site { get; set; }

    /// <summary>
    /// 支付方式信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(PaymentId))]
    public Payments? Payment { get; set; }
}
