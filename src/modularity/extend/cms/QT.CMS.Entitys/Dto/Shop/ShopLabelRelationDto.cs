using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;
/// <summary>
/// 商品标签关联
/// </summary>
public class ShopLabelRelationDto
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
    /// 所属标签
    /// </summary>
    [Display(Name = "所属标签")]
    public long labelId { get; set; }
}
