using QT.Common.Const;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 文章点赞
/// </summary>
[SugarTable("cms_article_like")]
[Tenant(ClaimConst.TENANTID)]
public class ArticleLike
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// 所属文章ID
    /// </summary>
    [Display(Name = "所属文章")]
    [ForeignKey("Article")]
    public long ArticleId { get; set; }

    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    public string UserId { get; set; }

    /// <summary>
    /// 点赞时间
    /// </summary>
    [Display(Name = "点赞时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 文章信息
    /// </summary>
    public Articles? Article { get; set; }
}
