using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 商品评价图片
/// </summary>
[SugarTable("cms_order_evaluate_album")]
public class OrderEvaluateAlbum
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属评价
    /// </summary>
    [Display(Name = "所属评价")]
    [ForeignKey("OrderEvaluate")]
    public long EvaluateId { get; set; }

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
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;


    /// <summary>
    /// 评价信息
    /// </summary>
    public OrderEvaluate? OrderEvaluate { get; set; }
}
