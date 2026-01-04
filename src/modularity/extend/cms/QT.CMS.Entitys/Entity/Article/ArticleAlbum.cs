using QT.Common.Const;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Channels;


namespace QT.CMS.Entitys;

/// <summary>
/// 文章相册
/// </summary>
[SugarTable("cms_article_album")]
[Tenant(ClaimConst.TENANTID)]
public class ArticleAlbum
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey =true,IsIdentity =true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属文章ID
    /// </summary>
    [Display(Name = "所属文章")]
    [ForeignKey("Article")]
    public long ArticleId { get; set; }

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
    /// 文章信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ArticleId))]
    public Articles? Article { get; set; }

}
