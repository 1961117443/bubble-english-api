using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 商品收藏(显示)
/// </summary>
public class ShopGoodsFavoriteDto : ShopGoodsFavoriteAddDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    public string? userId { get; set; }

    /// <summary>
    /// 商品标题
    /// </summary>
    [Display(Name = "商品标题")]
    [StringLength(512)]
    public string? title { get; set; }

    /// <summary>
    /// 商品图片
    /// </summary>
    [Display(Name = "商品图片")]
    [StringLength(512)]
    public string? imgUrl { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;
}

/// <summary>
/// 商品收藏(新增)
/// </summary>
public class ShopGoodsFavoriteAddDto
{
    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    public int siteId { get; set; }

    /// <summary>
    /// 所属商品ID
    /// </summary>
    [Display(Name = "所属商品")]
    public long goodsId { get; set; }
}
