using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 商品规格
/// </summary>
[SugarTable("cms_shop_goods_spec")]
[Tenant(ClaimConst.TENANTID)]
public class ShopGoodsSpec
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属商品ID
    /// </summary>
    [Display(Name = "所属商品")]
    [ForeignKey("ShopGoods")]
    public long GoodsId { get; set; }

    /// <summary>
    /// 所属规格ID
    /// </summary>
    [Display(Name = "所属规格")]
    public long SpecId { get; set; }

    /// <summary>
    /// 父规格ID
    /// </summary>
    [Display(Name = "父规格")]
    public long ParentId { get; set; } = 0;

    /// <summary>
    /// 规格标题
    /// </summary>
    [Display(Name = "标题")]
    [Required]
    [StringLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// 规格图片
    /// </summary>
    [Display(Name = "规格图片")]
    [StringLength(512)]
    public string? ImgUrl { get; set; }

    /// <summary>
    /// 商品信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(GoodsId))]
    public ShopGoods? ShopGoods { get; set; }
}
