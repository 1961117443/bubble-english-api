using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Order;

/// <summary>
/// 退换货商品(显示)
/// </summary>
public class OrderRefundGoodsDto : OrderRefundGoodsEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public string id { get; set; }

    /// <summary>
    /// 所属退换
    /// </summary>
    [Display(Name = "所属退换")]
    public long refundId { get; set; }

    /// <summary>
    /// 订单货品
    /// </summary>
    public OrderGoodsDto? orderGoods { get; set; }
}

/// <summary>
/// 退换货商品(编辑)
/// </summary>
public class OrderRefundGoodsEditDto
{
    /// <summary>
    /// 所属订单货品ID
    /// </summary>
    [Display(Name = "所属订单货品")]
    public long orderGoodsId { get; set; }
}
