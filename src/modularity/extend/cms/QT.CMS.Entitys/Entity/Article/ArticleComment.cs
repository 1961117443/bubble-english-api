using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 文章评论
/// </summary>
[SugarTable("cms_article_comment")]
[Tenant(ClaimConst.TENANTID)]
public class ArticleComment
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsIdentity =true,IsPrimaryKey =true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    public int SiteId { get; set; }

    /// <summary>
    /// 所属频道ID
    /// </summary>
    [Display(Name = "所属频道")]
    public int ChannelId { get; set; }

    /// <summary>
    /// 所属文章
    /// </summary>
    [Display(Name = "所属文章")]
    [ForeignKey("Article")]
    public long ArticleId { get; set; }

    /// <summary>
    /// 所属父类
    /// </summary>
    [Display(Name = "所属父类")]
    public long ParentId { get; set; }

    /// <summary>
    /// 首层主键
    /// </summary>
    [Display(Name = "首层主键")]
    public long RootId { get; set; }

    /// <summary>
    /// 评论用户
    /// </summary>
    [Display(Name = "评论用户")]
    [ForeignKey("User")]
    public string UserId { get; set; } = "";

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [StringLength(128)]
    public string? UserName { get; set; }

    /// <summary>
    /// 用户IP
    /// </summary>
    [Display(Name = "用户IP")]
    [StringLength(128)]
    public string? UserIp { get; set; }

    /// <summary>
    /// 被评论人ID
    /// </summary>
    [Display(Name = "被评论人")]
    public string AtUserId { get; set; } = "";

    /// <summary>
    /// 被评论人用户名
    /// </summary>
    [Display(Name = "被评论人用户名")]
    [StringLength(128)]
    public string? AtUserName { get; set; }

    /// <summary>
    /// 评论内容
    /// </summary>
    [Display(Name = "评论内容")]
    [StringLength(512)]
    public string? Content { get; set; }

    /// <summary>
    /// 点赞数量
    /// </summary>
    [Display(Name = "点赞数量")]
    public int LikeCount { get; set; } = 0;

    /// <summary>
    /// 状态0正常1隐藏
    /// </summary>
    [Display(Name = "状态")]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 已删除0否1是
    /// </summary>
    [Display(Name = "已删除")]
    public byte IsDelete { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 评论点赞列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ArticleCommentLike.CommentId))]
    public ICollection<ArticleCommentLike> CommentLikes { get; set; } = new List<ArticleCommentLike>();

    /// <summary>
    /// 文章信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ArticleId))]
    public Articles? Article { get; set; }

    ///// <summary>
    ///// 用户信息
    ///// </summary>
    //public ApplicationUser? User { get; set; }
}
