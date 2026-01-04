using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 评论点赞
/// </summary>
[SugarTable("cms_article_comment_like")]
[Tenant(ClaimConst.TENANTID)]
public class ArticleCommentLike
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsIdentity =true,IsPrimaryKey =true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属评论ID
    /// </summary>
    [Display(Name = "所属评论")]
    [ForeignKey("Comment")]
    public long CommentId { get; set; }

    /// <summary>
    /// 所属用户
    /// </summary>
    [Display(Name = "所属用户")]
    public string UserId { get; set; }

    /// <summary>
    /// 点赞时间
    /// </summary>
    [Display(Name = "点赞时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 评论信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(CommentId))]
    public ArticleComment? Comment { get; set; }
}
