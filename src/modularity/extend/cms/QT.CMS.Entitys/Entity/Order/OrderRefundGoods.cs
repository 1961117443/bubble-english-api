using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace QT.CMS.Entitys;

/// <summary>
/// 退换货商品
/// </summary>
[SugarTable("cms_order_refund_goods")]
public class OrderRefundGoods
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属退换ID
    /// </summary>
    [Display(Name = "所属退换")]
    [ForeignKey("OrderRefund")]
    public long RefundId { get; set; }

    /// <summary>
    /// 所属订单货品
    /// </summary>
    [Display(Name = "所属订单货品")]
    [ForeignKey("OrderGoods")]
    public long OrderGoodsId { get; set; }


    /// <summary>
    /// 订单退换货信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(RefundId))]
    public OrderRefund? OrderRefund { get; set; }

    /// <summary>
    /// 订单货品
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(OrderGoodsId))]
    public OrderGoods? OrderGoods { get; set; }

}
