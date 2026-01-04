using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys.Dto.Order;

/// <summary>
/// 订单货品(显示)
/// </summary>
public class OrderGoodsDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public string id { get; set; }

    /// <summary>
    /// 所属订单
    /// </summary>
    [Display(Name = "所属订单")]
    public long orderId { get; set; }

    /// <summary>
    /// 所属商品
    /// </summary>
    [Display(Name = "所属商品")]
    public long goodsId { get; set; }

    /// <summary>
    /// 所属货品ID
    /// </summary>
    [Display(Name = "所属货品")]
    public long productId { get; set; }

    /// <summary>
    /// 商品编号
    /// </summary>
    [Display(Name = "商品编号")]
    [StringLength(128)]
    public string? goodsNo { get; set; }

    /// <summary>
    /// 商品标题
    /// </summary>
    [Display(Name = "商品标题")]
    [StringLength(255)]
    public string? goodsTitle { get; set; }

    /// <summary>
    /// 规格JSON描述
    /// </summary>
    [Display(Name = "规格JSON描述")]
    public string? specText { get; set; }

    /// <summary>
    /// 商品图片
    /// </summary>
    [Display(Name = "商品图片")]
    [StringLength(512)]
    public string? imgUrl { get; set; }

    /// <summary>
    /// 商品价格
    /// </summary>
    [Display(Name = "商品价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal goodsPrice { get; set; } = 0M;

    /// <summary>
    /// 实际价格
    /// </summary>
    [Display(Name = "实际价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal realPrice { get; set; } = 0M;

    /// <summary>
    /// 购买数量
    /// </summary>
    [Display(Name = "购买数量")]
    public int quantity { get; set; } = 1;

    /// <summary>
    /// 重量(克)
    /// </summary>
    [Display(Name = "重量(克)")]
    public int weight { get; set; } = 0;

    /// <summary>
    /// 0未发1已发2已退
    /// </summary>
    [Display(Name = "发货状态")]
    public byte deliveryStatus { get; set; } = 0;

    /// <summary>
    /// 0未评1已评
    /// </summary>
    [Display(Name = "评价状态")]
    public byte evaluateStatus { get; set; } = 0;
}

/// <summary>
/// 订单货品(编辑)
/// </summary>
public class OrderGoodsEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public string id { get; set; }

    /// <summary>
    /// 所属订单ID
    /// </summary>
    [Display(Name = "所属订单")]
    public long orderId { get; set; }

    /// <summary>
    /// 所属货品
    /// </summary>
    [Display(Name = "所属货品")]
    public long productId { get; set; }

    /// <summary>
    /// 购买数量
    /// </summary>
    [Display(Name = "购买数量")]
    public int quantity { get; set; } = 1;
}
