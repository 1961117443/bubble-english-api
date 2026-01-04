using QT.Common.Const;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;


namespace QT.CMS.Entitys;

/// <summary>
/// 文章附件
/// </summary>
[SugarTable("cms_article_attach")]
[Tenant(ClaimConst.TENANTID)]
public class ArticleAttach
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属文章ID
    /// </summary>
    [Display(Name = "所属文章")]
    [ForeignKey("Article")]
    public long ArticleId { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    [Display(Name = "文件名")]
    [StringLength(255)]
    public string? FileName { get; set; }

    /// <summary>
    /// 文件路径
    /// </summary>
    [Display(Name = "文件路径")]
    [StringLength(512)]
    public string? FilePath { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    [Display(Name = "文件大小")]
    public int FileSize { get; set; }

    /// <summary>
    /// 扩展名
    /// </summary>
    [Display(Name = "扩展名")]
    [StringLength(128)]
    public string? FileExt { get; set; }

    /// <summary>
    /// 下载所需积分
    /// </summary>
    [Display(Name = "下载所需积分")]
    public int Point { get; set; } = 0;

    /// <summary>
    /// 下载次数
    /// </summary>
    [Display(Name = "下载次数")]
    public int DownCount { get; set; }

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

    [Display(Name = "会员下载")]
    public int IsMember { get; set; }

}
