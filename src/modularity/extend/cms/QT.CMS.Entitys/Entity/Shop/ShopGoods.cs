using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 商品信息
/// </summary>
[SugarTable("cms_shop_goods")]
[Tenant(ClaimConst.TENANTID)]
public class ShopGoods
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属站点
    /// </summary>
    [Display(Name = "所属站点")]
    public int SiteId { get; set; }

    /// <summary>
    /// 所属品牌
    /// </summary>
    [Display(Name = "所属品牌")]
    [ForeignKey("Brand")]
    public long BrandId { get; set; }

    /// <summary>
    /// 扩展属性模型ID
    /// </summary>
    [Display(Name = "扩展属性模型")]
    public long FieldId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required]
    [StringLength(255)]
    public string? Title { get; set; }

    /// <summary>
    /// 副标题
    /// </summary>
    [Display(Name = "副标题")]
    [StringLength(512)]
    public string? SubTitle { get; set; }

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
    /// 计件单位
    /// </summary>
    [Display(Name = "计件单位")]
    [StringLength(30)]
    public string? Unit { get; set; }

    /// <summary>
    /// 赠送积分
    /// </summary>
    [Display(Name = "赠送积分")]
    public int Point { get; set; } = 0;

    /// <summary>
    /// 赠送经验值
    /// </summary>
    [Display(Name = "赠送经验值")]
    public int Exp { get; set; } = 0;

    /// <summary>
    /// 图片地址
    /// </summary>
    [Display(Name = "图片地址")]
    [StringLength(512)]
    public string? ImgUrl { get; set; }

    /// <summary>
    /// SEO标题
    /// </summary>
    [Display(Name = "SEO标题")]
    [StringLength(255)]
    public string? SeoTitle { get; set; }

    /// <summary>
    /// SEO关健字
    /// </summary>
    [Display(Name = "SEO关健字")]
    [StringLength(512)]
    public string? SeoKeyword { get; set; }

    /// <summary>
    /// SEO描述
    /// </summary>
    [Display(Name = "SEO描述")]
    [StringLength(512)]
    public string? SeoDescription { get; set; }

    /// <summary>
    /// 内容摘要
    /// </summary>
    [Display(Name = "内容摘要")]
    [StringLength(255)]
    public string? Zhaiyao { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "内容")]
    [Column(TypeName = "text")]
    public string? Content { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int SortId { get; set; } = 99;

    /// <summary>
    /// 是否免运费
    /// </summary>
    [Display(Name = "是否免运费")]
    [Range(0, 9)]
    public byte IsDeliveryFee { get; set; } = 0;

    /// <summary>
    /// 状态0上架1下架
    /// </summary>
    [Display(Name = "状态")]
    [Range(0, 9)]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 审核状态：0正常1待审核
    /// </summary>
    [Display(Name = "审核状态")]
    [Range(0, 9)]
    public byte IsLock { get; set; } = 0;

    /// <summary>
    /// 浏览总数
    /// </summary>
    [Display(Name = "浏览总数")]
    public int Click { get; set; } = 0;

    /// <summary>
    /// 评价总数
    /// </summary>
    [Display(Name = "评价总数")]
    public int EvaluateCount { get; set; } = 0;

    /// <summary>
    /// 收藏总数
    /// </summary>
    [Display(Name = "收藏总数")]
    public int FavoriteCount { get; set; } = 0;

    /// <summary>
    /// 销售总数
    /// </summary>
    [Display(Name = "销售总数")]
    public int SaleCount { get; set; } = 0;

    /// <summary>
    /// 上架时间
    /// </summary>
    [Display(Name = "上架时间")]
    public DateTime? UpTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(128)]
    public string? AddBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    [StringLength(128)]
    public string? UpdateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 扩展属性值列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ShopGoodsFieldValue.GoodsId))]
    public List<ShopGoodsFieldValue> FieldValues { get; set; }

    // <summary>
    /// 类别关联列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ShopCategoryRelation.GoodsId))]
    public List<ShopCategoryRelation> CategoryRelations { get; set; }

    /// <summary>
    /// 标签关联列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ShopLabelRelation.GoodsId))] 
    public List<ShopLabelRelation> LabelRelations { get; set; }

    /// <summary>
    /// 商品货品列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ShopGoodsProduct.GoodsId))] 
    public List<ShopGoodsProduct> GoodsProducts { get; set; }

    /// <summary>
    /// 商品规格列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ShopGoodsSpec.GoodsId))] 
    public List<ShopGoodsSpec> GoodsSpecs { get; set; }

    /// <summary>
    /// 相册列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ShopGoodsAlbum.GoodsId))] 
    public List<ShopGoodsAlbum> GoodsAlbums { get; set; }

    /// <summary>
    /// 商品品牌
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(BrandId))]
    public ShopBrand? Brand { get; set; }
}
