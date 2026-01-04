using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 购物车(统计)
/// </summary>
public class ShopCartTotalDto
{
    /// <summary>
    /// 商品总数量
    /// </summary>
    public int totalQuantity { get; set; }

    /// <summary>
    /// 商品总重量(克)
    /// </summary>
    public int totalWeight { get; set; }

    /// <summary>
    /// 所需兑换积分
    /// </summary>
    public int totalPoint { get; set; } = 0;

    /// <summary>
    /// 商品应付金额
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal payableAmount { get; set; } = 0;

    /// <summary>
    /// 商品实付金额
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal realAmount { get; set; } = 0;

    /// <summary>
    /// 优惠券金额
    /// </summary>
    public decimal couponAmount { get; set; } = 0M;

    /// <summary>
    /// 促销活动金额
    /// </summary>
    public decimal promotionAmount { get; set; } = 0;

    /// <summary>
    /// 运费金额
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal freightAmount { get; set; } = 0;

    /// <summary>
    /// 订单实付金额
    /// </summary>
    public decimal orderAmount { get; set; } = 0;

    /// <summary>
    /// 是否免运费(0否1是全部商品免运费)
    /// </summary>
    public byte freeFreight { get; set; } = 1;

    /// <summary>
    /// 购物车商品列表
    /// </summary>
    public ICollection<ShopCartDto> cartList { get; set; } = new List<ShopCartDto>();

    /// <summary>
    /// 促销活动信息
    /// </summary>
    public ShopPromotionDto? promotion { get; set; }
}

/// <summary>
/// 购物车(显示)
/// </summary>
public class ShopCartDto : ShopCartEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 所属站点
    /// </summary>
    [Display(Name = "所属站点")]
    public int siteId { get; set; }

    /// <summary>
    /// 所属用户
    /// </summary>
    [Display(Name = "所属用户")]
    public string userId { get; set; }

    /// <summary>
    /// 状态0有效1失效
    /// </summary>
    public byte status { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; }

    /// <summary>
    /// 商品ID
    /// </summary>
    [Display(Name = "商品ID")]
    public long goodsId { get; set; }

    /// <summary>
    /// 商品标题
    /// </summary>
    [Display(Name = "商品标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(255)]
    public string? title { get; set; }

    /// <summary>
    /// 商品规格
    /// </summary>
    [Display(Name = "商品规格")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? specText { get; set; }

    /// <summary>
    /// 商品图片
    /// </summary>
    [Display(Name = "商品图片")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(512)]
    public string? imgUrl { get; set; }

    /// <summary>
    /// 市场价格
    /// </summary>
    [Display(Name = "市场价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal marketPrice { get; set; }

    /// <summary>
    /// 销售价格
    /// </summary>
    [Display(Name = "销售价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal sellPrice { get; set; }

    /// <summary>
    /// 会员价格
    /// </summary>
    [Display(Name = "会员价格")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal groupPrice { get; set; }

    /// <summary>
    /// 重量(克)
    /// </summary>
    [Display(Name = "重量(克)")]
    public int weight { get; set; } = 0;

    /// <summary>
    /// 库存数量
    /// </summary>
    [Display(Name = "库存数量")]
    public int stockQuantity { get; set; } = 0;
}

/// <summary>
/// 购物车(编辑)
/// </summary>
public class ShopCartEditDto
{
    /// <summary>
    /// 商品货品
    /// </summary>
    [Display(Name = "商品货品")]
    public long productId { get; set; }

    /// <summary>
    /// 购买数量
    /// </summary>
    [Display(Name = "购买数量")]
    public int quantity { get; set; } = 1;
}
