using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 分类规格筛选(用于查询)
/// </summary>
[SugarTable("cms_shop_category_spec")]
[Tenant(ClaimConst.TENANTID)]
public class ShopCategorySpec
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属分类
    /// </summary>
    [Display(Name = "所属分类")]
    public long CategoryId { get; set; }

    /// <summary>
    /// 所属商品
    /// </summary>
    [Display(Name = "所属商品")]
    public long GoodsId { get; set; }

    /// <summary>
    /// 规格ID(一级)
    /// </summary>
    [Display(Name = "所属规格")]
    public long SpecId { get; set; }
}
