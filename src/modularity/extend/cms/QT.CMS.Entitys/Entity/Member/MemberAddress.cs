using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 会员收货地址
/// </summary>
[SugarTable("cms_member_address")]
public class MemberAddress
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属用户
    /// </summary>
    [Display(Name = "所属用户")]
    public string UserId { get; set; }

    /// <summary>
    /// 收件人姓名
    /// </summary>
    [Display(Name = "收件人姓名")]
    [StringLength(30)]
    public string? AcceptName { get; set; }

    /// <summary>
    /// 所在省
    /// </summary>
    [Display(Name = "所在省")]
    [StringLength(30)]
    public string? Province { get; set; }

    /// <summary>
    /// 所在市
    /// </summary>
    [Display(Name = "所在市")]
    [StringLength(30)]
    public string? City { get; set; }

    /// <summary>
    /// 所在区
    /// </summary>
    [Display(Name = "所在区")]
    [StringLength(30)]
    public string? Area { get; set; }

    /// <summary>
    /// 详细地址
    /// </summary>
    [Display(Name = "详细地址")]
    [StringLength(512)]
    public string? Address { get; set; }

    /// <summary>
    /// 固定电话
    /// </summary>
    [Display(Name = "固定电话")]
    [StringLength(30)]
    public string? Telphone { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    [Display(Name = "手机号码")]
    [StringLength(30)]
    public string? Mobile { get; set; }

    /// <summary>
    /// 邮政编号
    /// </summary>
    [Display(Name = "邮政编号")]
    [StringLength(30)]
    public string? Zip { get; set; }

    /// <summary>
    /// 是否默认地址
    /// </summary>
    [Display(Name = "是否默认")]
    public byte IsDefault { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;
}
