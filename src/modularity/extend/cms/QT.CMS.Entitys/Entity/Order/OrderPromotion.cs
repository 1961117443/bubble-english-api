using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 订单促销活动
/// </summary>
[SugarTable("cms_order_promotion")]
public class OrderPromotion
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 订单ID
    /// </summary>
    [Display(Name = "所属订单")]
    [ForeignKey("Order")]
    public long OrderId { get; set; }

    /// <summary>
    /// 促销活动ID
    /// </summary>
    [Display(Name = "促销活动")]
    [ForeignKey("Promotion")]
    public long PromotionId { get; set; }


    /// <summary>
    /// 订单信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(OrderId))]
    public Orders? Order { get; set; }

    /// <summary>
    /// 促销活动信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(PromotionId))]
    public ShopPromotion? Promotion { get; set; }
}
