using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 商品相册
/// </summary>
[SugarTable("cms_shop_goods_album")]
[Tenant(ClaimConst.TENANTID)]
public class ShopGoodsAlbum
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
    /// 缩略图
    /// </summary>
    [Display(Name = "缩略图")]
    [StringLength(512)]
    public string? ThumbPath { get; set; }

    /// <summary>
    /// 原图
    /// </summary>
    [Display(Name = "原图")]
    [StringLength(512)]
    public string? OriginalPath { get; set; }

    /// <summary>
    /// 图片描述
    /// </summary>
    [Display(Name = "图片描述")]
    [StringLength(512)]
    public string? Remark { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int SortId { get; set; } = 99;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 商品信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(GoodsId))]
    public ShopGoods? ShopGoods { get; set; }
}
