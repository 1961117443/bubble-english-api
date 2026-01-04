using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 优惠券历史记录(显示)
/// </summary>
public class ShopCouponHistoryDto : ShopCouponHistoryEditDto
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
    /// 优惠券信息
    /// </summary>
    public ShopCouponDto? shopCoupon { get; set; }
}

/// <summary>
/// 优惠券历史记录(编辑)
/// </summary>
public class ShopCouponHistoryEditDto
{
    /// <summary>
    /// 所属优惠券ID
    /// </summary>
    [Display(Name = "所属优惠券")]
    public long couponId { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    [Display(Name = "订单号")]
    [StringLength(128)]
    public string? orderNo { get; set; }

    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    public string userId { get; set; }

    /// <summary>
    /// 获取类型0后台赠1主动领
    /// </summary>
    [Display(Name = "获取类型")]
    public byte type { get; set; } = 0;

    /// <summary>
    /// 使用状态0未用1已用
    /// </summary>
    [Display(Name = "使用状态")]
    public byte isUse { get; set; } = 0;

    /// <summary>
    /// 状态0正常1禁用
    /// </summary>
    [Display(Name = "状态")]
    public byte status { get; set; } = 0;

    /// <summary>
    /// 使用时间
    /// </summary>
    [Display(Name = "使用时间")]
    public DateTime? useTime { get; set; }
}
