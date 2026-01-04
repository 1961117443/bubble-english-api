using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys.Dto.Shop;

/// <summary>
/// 会员组货品价格
/// </summary>
public class ShopGoodsGroupPriceDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 所属组别ID
    /// </summary>
    [Display(Name = "所属组别")]
    public int groupId { get; set; }

    /// <summary>
    /// 所属货品ID
    /// </summary>
    [Display(Name = "所属货品")]
    public long productId { get; set; }

    /// <summary>
    /// 货品金额
    /// </summary>
    [Display(Name = "金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal price { get; set; } = 0;
}
