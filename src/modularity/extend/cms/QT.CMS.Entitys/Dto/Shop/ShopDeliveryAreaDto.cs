using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 配送区域价格
/// </summary>
public class ShopDeliveryAreaDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public int id { get; set; }

    /// <summary>
    /// 配送方式ID
    /// </summary>
    [Display(Name = "所属配送方式")]
    public int deliveryId { get; set; }

    /// <summary>
    /// 省份ID
    /// </summary>
    [Display(Name = "所属省份")]
    public int province { get; set; }

    /// <summary>
    /// 首重价格
    /// </summary>
    [Display(Name = "首重价格")]
    public decimal firstPrice { get; set; } = 0;

    /// <summary>
    /// 续重价格
    /// </summary>
    [Display(Name = "续重价格")]
    public decimal secondPrice { get; set; } = 0;
}
