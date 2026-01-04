using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 订单货品
/// </summary>
[SugarTable("cms_order_goods")]
public class OrderGoods
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属订单ID
    /// </summary>
    [Display(Name = "所属订单")]
    [ForeignKey("Order")]
    public long OrderId { get; set; }

    /// <summary>
    /// 所属商品ID
    /// </summary>
    [Display(Name = "所属商品")]
    public long GoodsId { get; set; }

    /// <summary>
    /// 所属货品ID
    /// </summary>
    [Display(Name = "所属货品")]
    public long ProductId { get; set; }

    /// <summary>
    /// 商品编号
    /// </summary>
    [Display(Name = "商品编号")]
    [StringLength(128)]
    public string? GoodsNo { get; set; }

    /// <summary>
    /// 商品标题
    /// </summary>
    [Display(Name = "商品标题")]
    [StringLength(255)]
    public string? GoodsTitle { get; set; }

    /// <summary>
    /// 规格JSON描述
    /// </summary>
    [Display(Name = "规格JSON描述")]
    public string? SpecText { get; set; }

    /// <summary>
    /// 商品图片
    /// </summary>
    [Display(Name = "商品图片")]
    [StringLength(512)]
    public string? ImgUrl { get; set; }

    /// <summary>
    /// 商品价格
    /// </summary>
    [Display(Name = "商品价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal GoodsPrice { get; set; } = 0M;

    /// <summary>
    /// 实际价格
    /// </summary>
    [Display(Name = "实际价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal RealPrice { get; set; } = 0M;

    /// <summary>
    /// 购买数量
    /// </summary>
    [Display(Name = "购买数量")]
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// 重量(克)
    /// </summary>
    [Display(Name = "重量(克)")]
    public int Weight { get; set; } = 0;

    /// <summary>
    /// 0未发1已发2已退
    /// </summary>
    [Display(Name = "发货状态")]
    public byte DeliveryStatus { get; set; } = 0;

    /// <summary>
    /// 0未评1已评
    /// </summary>
    [Display(Name = "评价状态")]
    public byte EvaluateStatus { get; set; } = 0;


    /// <summary>
    /// 订单信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(OrderId))]
    public Orders? Order { get; set; }
}
