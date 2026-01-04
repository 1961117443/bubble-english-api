using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 商品货品
/// </summary>
[SugarTable("cms_shop_goods_product")]
[Tenant(ClaimConst.TENANTID)]
public class ShopGoodsProduct
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属商品
    /// </summary>
    [Display(Name = "所属商品")]
    [ForeignKey("ShopGoods")]
    public long GoodsId { get; set; }

    /// <summary>
    /// 商品货号
    /// </summary>
    [Display(Name = "商品货号")]
    [StringLength(128)]
    public string? GoodsNo { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    [Display(Name = "库存数量")]
    public int StockQuantity { get; set; } = 0;

    /// <summary>
    /// 起订数量0不限制
    /// </summary>
    [Display(Name = "起订数量")]
    public int MinQuantity { get; set; } = 0;

    /// <summary>
    /// 市场价格
    /// </summary>
    [Display(Name = "市场价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal MarketPrice { get; set; } = 0;

    /// <summary>
    /// 销售价格
    /// </summary>
    [Display(Name = "销售价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SellPrice { get; set; } = 0;

    /// <summary>
    /// 成本价格
    /// </summary>
    [Display(Name = "成本价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal CostPrice { get; set; } = 0;

    /// <summary>
    /// 重量(克)
    /// </summary>
    [Display(Name = "重量(克)")]
    public int Weight { get; set; } = 0;

    /// <summary>
    /// 规格ID逗号分隔开
    /// </summary>
    [Display(Name = "规格ID集合")]
    public string? SpecIds { get; set; }

    /// <summary>
    /// 规格描述JSON
    /// </summary>
    [Display(Name = "规格文字集合")]
    public string? SpecText { get; set; }

    [Display(Name = "关联图片")]
    [StringLength(512)]
    public string? ImgUrl { get; set; }

    /// <summary>
    /// 会员组价格
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ShopGoodsGroupPrice.ProductId))]
    public List<ShopGoodsGroupPrice> GroupPrices { get; set; }

    /// <summary>
    /// 商品信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(GoodsId))]
    public ShopGoods? ShopGoods { get; set; }
}
