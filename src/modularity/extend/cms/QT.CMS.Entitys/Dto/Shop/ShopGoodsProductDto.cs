using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 商品货品(反查商品)
/// </summary>
public class ShopGoodsProductListDto : ShopGoodsProductDto
{
    /// <summary>
    /// 商品信息
    /// </summary>
    public ShopGoodsListDto? shopGoods { get; set; }
}

/// <summary>
/// 商品货品(增改查)
/// </summary>
public class ShopGoodsProductDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 所属商品
    /// </summary>
    [Display(Name = "所属商品")]
    public long goodsId { get; set; }

    /// <summary>
    /// 商品货号
    /// </summary>
    [Display(Name = "商品货号")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? goodsNo { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    [Display(Name = "库存数量")]
    public int stockQuantity { get; set; } = 0;

    /// <summary>
    /// 起订数量0不限制
    /// </summary>
    [Display(Name = "起订数量")]
    public int minQuantity { get; set; } = 1;

    /// <summary>
    /// 市场价格
    /// </summary>
    [Display(Name = "市场价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal marketPrice { get; set; } = 0;

    /// <summary>
    /// 销售价格
    /// </summary>
    [Display(Name = "销售价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal sellPrice { get; set; } = 0;

    /// <summary>
    /// 成本价格
    /// </summary>
    [Display(Name = "成本价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal costPrice { get; set; } = 0;

    /// <summary>
    /// 重量(克)
    /// </summary>
    [Display(Name = "重量(克)")]
    public int weight { get; set; } = 0;

    /// <summary>
    /// 规格ID逗号分隔开
    /// </summary>
    [Display(Name = "规格ID集合")]
    public string? specIds { get; set; }

    /// <summary>
    /// 规格描述JSON
    /// </summary>
    [Display(Name = "规格文字集合")]
    public string? specText { get; set; }

    /// <summary>
    /// 关联图片
    /// </summary>
    [Display(Name = "关联图片")]
    [StringLength(512)]
    public string? imgUrl { get; set; }

    /// <summary>
    /// 会员组价格
    /// </summary>
    public ICollection<ShopGoodsGroupPriceDto> groupPrices { get; set; } = new List<ShopGoodsGroupPriceDto>();
}

/// <summary>
/// 商品货品(前端)
/// </summary>
public class ShopGoodsProductClientDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 所属商品
    /// </summary>
    [Display(Name = "所属商品")]
    public long goodsId { get; set; }

    /// <summary>
    /// 商品货号
    /// </summary>
    [Display(Name = "商品货号")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? goodsNo { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    [Display(Name = "库存数量")]
    public int stockQuantity { get; set; } = 0;

    /// <summary>
    /// 起订数量0不限制
    /// </summary>
    [Display(Name = "起订数量")]
    public int minQuantity { get; set; } = 1;

    /// <summary>
    /// 市场价格
    /// </summary>
    [Display(Name = "市场价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal marketPrice { get; set; } = 0;

    /// <summary>
    /// 销售价格
    /// </summary>
    [Display(Name = "销售价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal sellPrice { get; set; } = 0;

    /// <summary>
    /// 重量(克)
    /// </summary>
    [Display(Name = "重量(克)")]
    public int weight { get; set; } = 0;

    /// <summary>
    /// 规格ID逗号分隔开
    /// </summary>
    [Display(Name = "规格ID集合")]
    public string? specIds { get; set; }

    /// <summary>
    /// 规格描述JSON
    /// </summary>
    [Display(Name = "规格文字集合")]
    public string? specText { get; set; }

    /// <summary>
    /// 关联图片
    /// </summary>
    [Display(Name = "关联图片")]
    [StringLength(512)]
    public string? imgUrl { get; set; }
}
