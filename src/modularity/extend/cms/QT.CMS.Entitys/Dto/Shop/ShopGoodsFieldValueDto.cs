using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 商品扩展属性值
/// </summary>
public class ShopGoodsFieldValueDto
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
    /// 扩展属性选项ID
    /// </summary>
    [Display(Name = "扩展属性选项")]
    public long optionId { get; set; }

    /// <summary>
    /// 选项名
    /// </summary>
    [Display(Name = "选项名")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? optionName { get; set; }

    /// <summary>
    /// 选项值
    /// </summary>
    [Display(Name = "选项值")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(512)]
    public string? optionValue { get; set; }
}
