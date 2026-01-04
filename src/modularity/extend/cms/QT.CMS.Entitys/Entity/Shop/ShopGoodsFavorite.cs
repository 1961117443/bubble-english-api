using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 商品收藏
/// </summary>
[SugarTable("cms_shop_goods_favorite")]
[Tenant(ClaimConst.TENANTID)]
public class ShopGoodsFavorite
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    public int SiteId { get; set; }

    /// <summary>
    /// 所属商品ID
    /// </summary>
    [Display(Name = "所属商品")]
    [ForeignKey("ShopGoods")]
    public long GoodsId { get; set; }

    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    public string UserId { get; set; }

    /// <summary>
    /// 商品标题
    /// </summary>
    [Display(Name = "商品标题")]
    [StringLength(512)]
    public string? Title { get; set; }

    /// <summary>
    /// 商品图片
    /// </summary>
    [Display(Name = "商品图片")]
    [StringLength(512)]
    public string? ImgUrl { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 商品信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(GoodsId))]
    public ShopGoods? ShopGoods { get; set; }
}
