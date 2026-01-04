using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 商城分类(显示)
/// </summary>
public class ShopCategoryDto : ShopCategoryEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

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
}

/// <summary>
/// 商城分类(树型)
/// </summary>
public class ShopCategoryListDto : ShopCategoryEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

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
    /// 子类列表
    /// </summary>
    public List<ShopCategoryListDto> children { get; set; } = new List<ShopCategoryListDto>();
}

/// <summary>
/// 商城分类(编辑)
/// </summary>
public class ShopCategoryEditDto
{
    /// <summary>
    /// 所属站点
    /// </summary>
    [Display(Name = "所属站点")]
    public int siteId { get; set; }

    /// <summary>
    /// 所属父类
    /// </summary>
    [Display(Name = "所属父类")]
    public long parentId { get; set; } = 0;

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? title { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    [Display(Name = "图片地址")]
    [StringLength(512)]
    public string? imgUrl { get; set; }

    /// <summary>
    /// 外部链接
    /// </summary>
    [Display(Name = "外部链接")]
    [StringLength(512)]
    public string? linkUrl { get; set; }

    /// <summary>
    /// 内容介绍
    /// </summary>
    [Display(Name = "内容介绍")]
    public string? content { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;

    /// <summary>
    /// SEO标题
    /// </summary>
    [Display(Name = "SEO标题")]
    [StringLength(512)]
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
    /// 状态0正常1禁用
    /// </summary>
    [Display(Name = "状态")]
    [Range(0, 9)]
    public byte status { get; set; } = 0;
}

/// <summary>
/// 商品分类(带商品)
/// </summary>
public class ShopCategoryGoodsClientDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    public string? title { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    [Display(Name = "图片地址")]
    public string? imgUrl { get; set; }

    /// <summary>
    /// 商品列表
    /// </summary>
    public IEnumerable<ShopGoodsClientDto> goodsList { get; set; } = new List<ShopGoodsClientDto>();
}
