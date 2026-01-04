using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 商品扩展属性选项
/// </summary>
[SugarTable("cms_shop_goods_field_value")]
[Tenant(ClaimConst.TENANTID)]
public class ShopGoodsFieldValue
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
    /// 扩展属性ID
    /// </summary>
    [Display(Name = "所属扩展属性")]
    public long OptionId { get; set; }

    /// <summary>
    /// 选项名
    /// </summary>
    [Display(Name = "选项名")]
    [StringLength(128)]
    public string? OptionName { get; set; }

    /// <summary>
    /// 选项值
    /// </summary>
    [Display(Name = "选项值")]
    [StringLength(512)]
    public string? OptionValue { get; set; }

    /// <summary>
    /// 商品信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(GoodsId))]
    public ShopGoods? ShopGoods { get; set; }
}
