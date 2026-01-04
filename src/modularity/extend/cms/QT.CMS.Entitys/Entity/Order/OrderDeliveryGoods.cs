using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace QT.CMS.Entitys;

/// <summary>
/// 订单发货货品
/// </summary>
[SugarTable("cms_order_delivery_goods")]
public class OrderDeliveryGoods
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 发货ID
    /// </summary>
    [Display(Name = "发货ID")]
    [ForeignKey("OrderDelivery")]
    public long DeliveryId { get; set; }

    /// <summary>
    /// 订单货品ID
    /// </summary>
    [Display(Name = "所属订单货品")]
    [ForeignKey("OrderGoods")]
    public long OrderGoodsId { get; set; }


    /// <summary>
    /// 发货信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(DeliveryId))]
    public OrderDelivery? OrderDelivery { get; set; }

    /// <summary>
    /// 订单货品
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(OrderGoodsId))]
    public OrderGoods? OrderGoods { get; set; }
}
