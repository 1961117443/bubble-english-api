using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 分类规格筛选(用于查询)
/// </summary>
public class ShopCategorySpecDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 所属分类ID
    /// </summary>
    [Display(Name = "所属分类")]
    public long categoryId { get; set; }

    /// <summary>
    /// 所属商品ID
    /// </summary>
    [Display(Name = "所属商品")]
    public long goodsId { get; set; }

    /// <summary>
    /// 规格ID(一级)
    /// </summary>
    [Display(Name = "所属规格")]
    public long specId { get; set; }
}
