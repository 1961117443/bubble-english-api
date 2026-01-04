using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 会员组价格
/// </summary>
[SugarTable("cms_shop_goods_group_price")]
[Tenant(ClaimConst.TENANTID)]
public class ShopGoodsGroupPrice
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属组别ID
    /// </summary>
    [Display(Name = "所属组别")]
    public int GroupId { get; set; }

    /// <summary>
    /// 所属货品ID
    /// </summary>
    [Display(Name = "所属货品")]
    [ForeignKey("GoodsProduct")]
    public long ProductId { get; set; }

    /// <summary>
    /// 金额
    /// </summary>
    [Display(Name = "金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; } = 0;

    /// <summary>
    /// 商品货品
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ProductId))]
    public ShopGoodsProduct? GoodsProduct { get; set; }
}
