using QT.CMS.Entitys.Dto.Shop;
using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Order;

/// <summary>
/// 订单促销活动(显示)
/// </summary>
public class OrderPromotionDto : OrderPromotionEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public string id { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    [Display(Name = "订单号")]
    [StringLength(128)]
    public string? orderNo { get; set; }

    /// <summary>
    /// 促销活动标题
    /// </summary>
    [Display(Name = "促销活动标题")]
    [StringLength(128)]
    public string? promotionTitle { get; set; }

    /// <summary>
    /// 促销活动信息
    /// </summary>
    public ShopPromotionDto? promotion { get; set; }
}

/// <summary>
/// 订单促销活动(编辑)
/// </summary>
public class OrderPromotionEditDto
{
    /// <summary>
    /// 订单ID
    /// </summary>
    [Display(Name = "所属订单")]
    public string orderId { get; set; }

    /// <summary>
    /// 促销活动ID
    /// </summary>
    [Display(Name = "促销活动")]
    public string promotionId { get; set; }
}
