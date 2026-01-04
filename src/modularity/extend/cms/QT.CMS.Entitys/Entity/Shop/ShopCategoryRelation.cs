using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 商品类别关联
/// </summary>
[SugarTable("cms_shop_category_relation")]
[Tenant(ClaimConst.TENANTID)]
public class ShopCategoryRelation
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属商品
    /// </summary>
    [Display(Name = "所属商品")]
    [ForeignKey("ShopGoods")]
    public long GoodsId { get; set; }

    /// <summary>
    /// 所属分类
    /// </summary>
    [Display(Name = "所属分类")]
    [ForeignKey("ShopCategory")]
    public long CategoryId { get; set; }

    /// <summary>
    /// 商品信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(GoodsId))]
    public ShopGoods? ShopGoods { get; set; }

    /// <summary>
    /// 类别信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(CategoryId))]
    public ShopCategory? ShopCategory { get; set; }
}
