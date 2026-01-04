using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 购物车
/// </summary>
[SugarTable("cms_shop_cart")]
[Tenant(ClaimConst.TENANTID)]
public class ShopCart
{
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    public int SiteId { get; set; }

    /// <summary>
    /// 所属用户
    /// </summary>
    [Display(Name = "所属用户")]
    public string? UserId { get; set; }

    /// <summary>
    /// 商品货品ID
    /// </summary>
    [Display(Name = "商品货品")]
    public long ProductId { get; set; }

    /// <summary>
    /// 商品标题
    /// </summary>
    [Display(Name = "商品标题")]
    [StringLength(255)]
    public string? Title { get; set; }

    /// <summary>
    /// 商品规格
    /// </summary>
    [Display(Name = "商品规格")]
    public string? SpecText { get; set; }

    /// <summary>
    /// 商品图片
    /// </summary>
    [Display(Name = "商品图片")]
    [StringLength(512)]
    public string? ImgUrl { get; set; }

    /// <summary>
    /// 购买数量
    /// </summary>
    [Display(Name = "购买数量")]
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;
}
