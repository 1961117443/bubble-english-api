using QT.Common.Const;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 优惠券历史记录
/// </summary>
[SugarTable("cms_shop_coupon_history")]
[Tenant(ClaimConst.TENANTID)]
public class ShopCouponHistory
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属优惠券
    /// </summary>
    [Display(Name = "所属优惠券")]
    [ForeignKey("ShopCoupon")]
    public long CouponId { get; set; }

    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    public string? UserId { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    [Display(Name = "订单号")]
    [StringLength(128)]
    public string? OrderNo { get; set; }

    /// <summary>
    /// 获取类型0后台赠1主动领
    /// </summary>
    [Display(Name = "获取类型")]
    public byte Type { get; set; } = 0;

    /// <summary>
    /// 使用状态0未用1已用
    /// </summary>
    [Display(Name = "使用状态")]
    public byte IsUse { get; set; } = 0;

    /// <summary>
    /// 状态0正常1禁用
    /// </summary>
    [Display(Name = "状态")]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 使用时间
    /// </summary>
    [Display(Name = "使用时间")]
    public DateTime? UseTime { get; set; }

    /// <summary>
    /// 优惠券信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(CouponId))]
    public ShopCoupon? ShopCoupon { get; set; }
}
