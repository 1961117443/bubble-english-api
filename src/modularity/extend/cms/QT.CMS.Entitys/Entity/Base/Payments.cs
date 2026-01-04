using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 支付方式
/// </summary>
[SugarTable("cms_payments")]
public class Payments
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 支付名称
    /// </summary>
    [Display(Name = "支付名称")]
    [Required]
    [StringLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// 显示图片
    /// </summary>
    [Display(Name = "显示图片")]
    [StringLength(512)]
    public string? ImgUrl { get; set; }

    /// <summary>
    /// 备注说明
    /// </summary>
    [Display(Name = "备注说明")]
    [StringLength(512)]
    public string? Remark { get; set; }

    /// <summary>
    /// 收款类型0线下1余额2微信3支付宝
    /// </summary>
    [Display(Name = "收款类型")]
    [Range(0, 9)]
    public byte Type { get; set; } = 0;

    /// <summary>
    /// 支付接口页面
    /// </summary>
    [Display(Name = "支付接口页面")]
    [StringLength(512)]
    public string? PayUrl { get; set; }

    /// <summary>
    /// 支付通知页面
    /// </summary>
    [Display(Name = "支付通知页面")]
    [StringLength(512)]
    public string? NotifyUrl { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int SortId { get; set; } = 99;

    /// <summary>
    /// 状态0启用1关闭
    /// </summary>
    [Display(Name = "状态")]
    public byte Status { get; set; } = 0;
}
