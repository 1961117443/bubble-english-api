using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QT.CMS.Entitys.Dto.Member;

/// <summary>
/// 会员收货地址(显示)
/// </summary>
public class MemberAddressDto : MemberAddressEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [StringLength(30)]
    public string? userName { get; set; }
}

/// <summary>
/// 会员收货地址(编辑)
/// </summary>
public class MemberAddressEditDto
{
    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string userId { get; set; }

    /// <summary>
    /// 收件人姓名
    /// </summary>
    [Display(Name = "收件人姓名")]
    [Required(ErrorMessage ="{0}不可为空")]
    [StringLength(30)]
    public string? acceptName { get; set; }

    /// <summary>
    /// 所在省
    /// </summary>
    [Display(Name = "所在省")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(30)]
    public string? province { get; set; }

    /// <summary>
    /// 所在市
    /// </summary>
    [Display(Name = "所在市")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(30)]
    public string? city { get; set; }

    /// <summary>
    /// 所在区
    /// </summary>
    [Display(Name = "所在区")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(30)]
    public string? area { get; set; }

    /// <summary>
    /// 详细地址
    /// </summary>
    [Display(Name = "详细地址")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(512)]
    public string? address { get; set; }

    /// <summary>
    /// 固定电话
    /// </summary>
    [Display(Name = "固定电话")]
    [StringLength(30)]
    public string? telphone { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    [Display(Name = "手机号码")]
    [Required(ErrorMessage = "{0}不可为空")]
    [RegularExpression(@"^(13|14|15|16|18|19|17)\d{9}$")]
    public string? mobile { get; set; }

    /// <summary>
    /// 邮政编号
    /// </summary>
    [Display(Name = "邮政编号")]
    [StringLength(30)]
    public string? zip { get; set; }

    /// <summary>
    /// 是否默认
    /// </summary>
    [Display(Name = "是否默认")]
    public byte isDefault { get; set; } = 0;
}
