using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 商品规格
/// </summary>
public class ShopGoodsSpecDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 所属商品ID
    /// </summary>
    [Display(Name = "所属商品")]
    public long goodsId { get; set; }

    /// <summary>
    /// 所属规格ID
    /// </summary>
    [Display(Name = "所属规格")]
    public long specId { get; set; }

    /// <summary>
    /// 父规格ID
    /// </summary>
    [Display(Name = "父规格")]
    public long parentId { get; set; } = 0;

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? title { get; set; }

    /// <summary>
    /// 规格图片
    /// </summary>
    [Display(Name = "规格图片")]
    [StringLength(512)]
    public string? imgUrl { get; set; }
}
