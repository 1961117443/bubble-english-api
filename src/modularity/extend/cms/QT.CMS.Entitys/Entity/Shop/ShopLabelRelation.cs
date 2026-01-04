using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 商品标签关联
/// </summary>
[SugarTable("cms_shop_label_relation")]
[Tenant(ClaimConst.TENANTID)]
public class ShopLabelRelation
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
    /// 所属标签ID
    /// </summary>
    [Display(Name = "所属标签")]
    [ForeignKey("ShopLabel")]
    public long LabelId { get; set; }

    /// <summary>
    /// 商品信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(GoodsId))]
    public ShopGoods? ShopGoods { get; set; }

    /// <summary>
    /// 标签信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(LabelId))]
    public ShopLabel? ShopLabel { get; set; }
}
