using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 商品信息(显示)
/// </summary>
public class ShopGoodsDto : ShopGoodsEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 审核状态：0正常1待审核
    /// </summary>
    [Display(Name = "审核状态")]
    [Range(0, 9)]
    public byte isLock { get; set; } = 0;

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(128)]
    public string? addBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    [StringLength(128)]
    public string? updateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? updateTime { get; set; }

    /// <summary>
    /// 上架时间
    /// </summary>
    [Display(Name = "上架时间")]
    public DateTime? upTime { get; set; }

    /// <summary>
    /// 浏览数量
    /// </summary>
    [Display(Name = "浏览数量")]
    public int click { get; set; } = 0;

    /// <summary>
    /// 评价数量
    /// </summary>
    [Display(Name = "评价数量")]
    public int evaluateCount { get; set; } = 0;

    /// <summary>
    /// 收藏数量
    /// </summary>
    [Display(Name = "收藏数量")]
    public int favoriteCount { get; set; } = 0;

    /// <summary>
    /// 销售数量
    /// </summary>
    [Display(Name = "销售数量")]
    public int saleCount { get; set; } = 0;

    /// <summary>
    /// 类别标题，以逗号分隔
    /// </summary>
    public string? categoryTitle { get; set; } = string.Empty;

    /// <summary>
    /// 标签标题，以逗号分隔
    /// </summary>
    public string? labelTitle { get; set; } = string.Empty;

    /// <summary>
    /// 品牌标题
    /// </summary>
    public string? brandTitle { get; set; } = string.Empty;
}

/// <summary>
/// 商品信息(编辑)
/// </summary>
public class ShopGoodsEditDto : ShopGoodsBaseDto
{
    // <summary>
    /// 类别关联列表
    /// </summary>
    public ICollection<ShopCategoryRelationDto> categoryRelations { get; set; } = new List<ShopCategoryRelationDto>();

    /// <summary>
    /// 标签关联列表
    /// </summary>
    public ICollection<ShopLabelRelationDto> labelRelations { get; set; } = new List<ShopLabelRelationDto>();

    /// <summary>
    /// 扩展属性值列表
    /// </summary>
    public ICollection<ShopGoodsFieldValueDto> fieldValues { get; set; } = new List<ShopGoodsFieldValueDto>();

    /// <summary>
    /// 商品货品列表
    /// </summary>
    public ICollection<ShopGoodsProductDto> goodsProducts { get; set; } = new List<ShopGoodsProductDto>();

    /// <summary>
    /// 商品规格列表
    /// </summary>
    public ICollection<ShopGoodsSpecDto> goodsSpecs { get; set; } = new List<ShopGoodsSpecDto>();

    /// <summary>
    /// 相册列表
    /// </summary>
    public ICollection<ShopGoodsAlbumDto> goodsAlbums { get; set; } = new List<ShopGoodsAlbumDto>();
}

/// <summary>
/// 商品信息(货品列表)
/// </summary>
public class ShopGoodsListDto : ShopGoodsBaseDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 审核状态：0正常1待审核
    /// </summary>
    [Display(Name = "审核状态")]
    [Range(0, 9)]
    public byte isLock { get; set; } = 0;

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(128)]
    public string? addBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    [StringLength(128)]
    public string? updateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? updateTime { get; set; }

    /// <summary>
    /// 上架时间
    /// </summary>
    [Display(Name = "上架时间")]
    public DateTime? upTime { get; set; }

    /// <summary>
    /// 浏览数量
    /// </summary>
    [Display(Name = "浏览数量")]
    public int click { get; set; } = 0;

    /// <summary>
    /// 评价数量
    /// </summary>
    [Display(Name = "评价数量")]
    public int evaluateCount { get; set; } = 0;

    /// <summary>
    /// 收藏数量
    /// </summary>
    [Display(Name = "收藏数量")]
    public int favoriteCount { get; set; } = 0;

    /// <summary>
    /// 销售数量
    /// </summary>
    [Display(Name = "销售数量")]
    public int saleCount { get; set; } = 0;

    /// <summary>
    /// 类别标题，以逗号分隔
    /// </summary>
    public string? categoryTitle { get; set; }

    /// <summary>
    /// 标签标题，以逗号分隔
    /// </summary>
    public string? labelTitle { get; set; }

    /// <summary>
    /// 品牌标题
    /// </summary>
    public string? brandTitle { get; set; }

    // <summary>
    /// 类别关联列表
    /// </summary>
    public ICollection<ShopCategoryRelationDto> categoryRelations { get; set; } = new List<ShopCategoryRelationDto>();
}

/// <summary>
/// 商品信息(公共)
/// </summary>
public class ShopGoodsBaseDto
{
    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int siteId { get; set; }

    /// <summary>
    /// 所属品牌ID
    /// </summary>
    [Display(Name = "所属品牌")]
    [Required(ErrorMessage = "{0}不可为空")]
    public long brandId { get; set; }

    /// <summary>
    /// 扩展属性ID
    /// </summary>
    [Display(Name = "扩展属性")]
    public long? fieldId { get; set; } = 0;

    /// <summary>
    /// 商品标题
    /// </summary>
    [Display(Name = "商品标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(255)]
    public string? title { get; set; }

    /// <summary>
    /// 副标题
    /// </summary>
    [Display(Name = "副标题")]
    [StringLength(512)]
    public string? subTitle { get; set; }

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
    /// 计件单位
    /// </summary>
    [Display(Name = "计件单位")]
    [StringLength(30)]
    public string? unit { get; set; }

    /// <summary>
    /// 赠送积分
    /// </summary>
    [Display(Name = "赠送积分")]
    public int point { get; set; } = 0;

    /// <summary>
    /// 赠送经验值
    /// </summary>
    [Display(Name = "赠送经验值")]
    public int exp { get; set; } = 0;

    /// <summary>
    /// 图片地址
    /// </summary>
    [Display(Name = "图片地址")]
    [StringLength(512)]
    public string? imgUrl { get; set; }

    /// <summary>
    /// SEO标题
    /// </summary>
    [Display(Name = "SEO标题")]
    [StringLength(255)]
    public string? seoTitle { get; set; }

    /// <summary>
    /// SEO关健字
    /// </summary>
    [Display(Name = "SEO关健字")]
    [StringLength(512)]
    public string? seoKeyword { get; set; }

    /// <summary>
    /// SEO描述
    /// </summary>
    [Display(Name = "SEO描述")]
    [StringLength(512)]
    public string? seoDescription { get; set; }

    /// <summary>
    /// 内容摘要
    /// </summary>
    [Display(Name = "内容摘要")]
    [StringLength(255)]
    public string? zhaiyao { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "内容")]
    [Column(TypeName = "text")]
    public string? content { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;

    /// <summary>
    /// 是否免运费0否1是
    /// </summary>
    [Display(Name = "是否免运费")]
    [Range(0, 9)]
    public byte isDeliveryFee { get; set; } = 0;

    /// <summary>
    /// 状态0上架1下架
    /// </summary>
    [Display(Name = "状态")]
    [Range(0, 9)]
    public byte status { get; set; } = 0;
}

/// <summary>
/// 商品信息(前端)
/// </summary>
public class ShopGoodsClientDto {
    /// <summary>
    /// 自增ID
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// 所属站点ID
    /// </summary>
    public int siteId { get; set; }

    /// <summary>
    /// 所属品牌ID
    /// </summary>
    public long brandId { get; set; }

    /// <summary>
    /// 扩展属性ID
    /// </summary>
    public long? fieldId { get; set; } = 0;

    /// <summary>
    /// 创建人
    /// </summary>
    public string? addBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 浏览数量
    /// </summary>
    public int click { get; set; } = 0;

    /// <summary>
    /// 评价数量
    /// </summary>
    public int evaluateCount { get; set; } = 0;

    /// <summary>
    /// 收藏数量
    /// </summary>
    public int favoriteCount { get; set; } = 0;

    /// <summary>
    /// 销售数量
    /// </summary>
    public int saleCount { get; set; } = 0;

    /// <summary>
    /// 类别标题，以逗号分隔
    /// </summary>
    public string? categoryTitle { get; set; }

    /// <summary>
    /// 标签标题，以逗号分隔
    /// </summary>
    public string? labelTitle { get; set; }

    /// <summary>
    /// 品牌标题
    /// </summary>
    public string? brandTitle { get; set; }

    /// <summary>
    /// 商品标题
    /// </summary>
    public string? title { get; set; }

    /// <summary>
    /// 副标题
    /// </summary>
    public string? subTitle { get; set; }

    /// <summary>
    /// 商品货号
    /// </summary>
    public string? goodsNo { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    public int stockQuantity { get; set; } = 0;

    /// <summary>
    /// 市场价格
    /// </summary>
    public decimal marketPrice { get; set; } = 0;

    /// <summary>
    /// 销售价格
    /// </summary>
    public decimal sellPrice { get; set; } = 0;

    /// <summary>
    /// 重量(克)
    /// </summary>
    public int weight { get; set; } = 0;

    /// <summary>
    /// 计件单位
    /// </summary>
    public string? unit { get; set; }

    /// <summary>
    /// 赠送积分
    /// </summary>
    public int point { get; set; } = 0;

    /// <summary>
    /// 赠送经验值
    /// </summary>
    public int exp { get; set; } = 0;

    /// <summary>
    /// 图片地址
    /// </summary>
    public string? imgUrl { get; set; }

    /// <summary>
    /// SEO标题
    /// </summary>
    public string? seoTitle { get; set; }

    /// <summary>
    /// SEO关健字
    /// </summary>
    public string? seoKeyword { get; set; }

    /// <summary>
    /// SEO描述
    /// </summary>
    public string? seoDescription { get; set; }

    /// <summary>
    /// 内容摘要
    /// </summary>
    public string? zhaiyao { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string? content { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    public int sortId { get; set; } = 99;

    /// <summary>
    /// 是否免运费
    /// </summary>
    public byte isDeliveryFee { get; set; } = 0;

    /// <summary>
    /// 状态0上架1下架
    /// </summary>
    public byte status { get; set; } = 0;

    /// <summary>
    /// 审核状态：0正常1待审核
    /// </summary>
    public byte isLock { get; set; } = 0;

    // <summary>
    /// 类别关联列表
    /// </summary>
    public ICollection<ShopCategoryRelationDto> categoryRelations { get; set; } = new List<ShopCategoryRelationDto>();

    /// <summary>
    /// 标签关联列表
    /// </summary>
    public ICollection<ShopLabelRelationDto> labelRelations { get; set; } = new List<ShopLabelRelationDto>();

    /// <summary>
    /// 扩展属性值列表
    /// </summary>
    public ICollection<ShopGoodsFieldValueDto> fieldValues { get; set; } = new List<ShopGoodsFieldValueDto>();

    /// <summary>
    /// 商品货品列表
    /// </summary>
    public ICollection<ShopGoodsProductClientDto> goodsProducts { get; set; } = new List<ShopGoodsProductClientDto>();

    /// <summary>
    /// 商品规格列表
    /// </summary>
    public ICollection<ShopGoodsSpecDto> goodsSpecs { get; set; } = new List<ShopGoodsSpecDto>();

    /// <summary>
    /// 相册列表
    /// </summary>
    public ICollection<ShopGoodsAlbumDto> goodsAlbums { get; set; } = new List<ShopGoodsAlbumDto>();
}
