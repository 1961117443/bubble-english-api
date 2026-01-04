using QT.Common.Const;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 商品分类
/// </summary>
[SugarTable("cms_shop_category")]
[Tenant(ClaimConst.TENANTID)]
public class ShopCategory
{
    /// <summary>
    /// 自增ID
    /// </summary>
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
    /// 所属父类ID
    /// </summary>
    [Display(Name = "所属父类")]
    public long ParentId { get; set; } = 0;

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [StringLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    [Display(Name = "图片地址")]
    [StringLength(512)]
    public string? ImgUrl { get; set; }

    /// <summary>
    /// 外部链接
    /// </summary>
    [Display(Name = "外部链接")]
    [StringLength(512)]
    public string? LinkUrl { get; set; }

    /// <summary>
    /// 内容介绍
    /// </summary>
    [Display(Name = "内容介绍")]
    [Column(TypeName = "text")]
    public string? Content { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int SortId { get; set; } = 99;

    /// <summary>
    /// SEO标题
    /// </summary>
    [Display(Name = "SEO标题")]
    [StringLength(512)]
    public string? SeoTitle { get; set; }

    /// <summary>
    /// SEO关健字
    /// </summary>
    [Display(Name = "SEO关健字")]
    [StringLength(512)]
    public string? SeoKeyword { get; set; }

    /// <summary>
    /// SEO描述
    /// </summary>
    [Display(Name = "SEO描述")]
    [StringLength(512)]
    public string? SeoDescription { get; set; }

    /// <summary>
    /// 状态0正常1禁用
    /// </summary>
    [Display(Name = "状态")]
    [Range(0, 9)]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(30)]
    public string? AddBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    [StringLength(30)]
    public string? UpdateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? UpdateTime { get; set; }

    // <summary>
    /// 类别关联列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ShopCategoryRelation.CategoryId))]
    public ICollection<ShopCategoryRelation> CategoryRelations { get; set; } = new List<ShopCategoryRelation>();
}
