using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys.Dto.Article;

/// <summary>
/// 文章评论(显示)
/// </summary>
public class ArticleCommentDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    public int siteId { get; set; }

    /// <summary>
    /// 所属频道ID
    /// </summary>
    [Display(Name = "所属频道")]
    public int channelId { get; set; }

    /// <summary>
    /// 所属文章
    /// </summary>
    [Display(Name = "所属文章")]
    public long articleId { get; set; }

    /// <summary>
    /// 所属父类
    /// </summary>
    [Display(Name = "所属父类")]
    public long parentId { get; set; }

    /// <summary>
    /// 首层主键
    /// </summary>
    [Display(Name = "首层主键")]
    public long rootId { get; set; }

    /// <summary>
    /// 评论用户
    /// </summary>
    [Display(Name = "评论用户")]
    [ForeignKey("User")]
    public int userId { get; set; } = 0;

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    public string? userName { get; set; }

    /// <summary>
    /// 用户头像
    /// </summary>
    [Display(Name = "用户头像")]
    public string? userAvatar { get; set; }

    /// <summary>
    /// 用户IP
    /// </summary>
    [Display(Name = "用户IP")]
    public string? userIp { get; set; }

    /// <summary>
    /// 被评论人ID
    /// </summary>
    [Display(Name = "被评论人")]
    public int atUserId { get; set; } = 0;

    /// <summary>
    /// 被评论人用户名
    /// </summary>
    [Display(Name = "被评论人用户名")]
    public string? atUserName { get; set; }

    /// <summary>
    /// 评论内容
    /// </summary>
    [Display(Name = "评论内容")]
    public string? content { get; set; }

    /// <summary>
    /// 复制评论内容
    /// </summary>
    [Display(Name = "复制评论内容")]
    public string? copyContent { get; set; }

    /// <summary>
    /// 点赞数量
    /// </summary>
    [Display(Name = "点赞数量")]
    public int likeCount { get; set; } = 0;

    /// <summary>
    /// 状态0正常1隐藏
    /// </summary>
    [Display(Name = "状态")]
    public byte status { get; set; } = 0;

    /// <summary>
    /// 已删除0否1是
    /// </summary>
    [Display(Name = "已删除")]
    public byte isDelete { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 类别深度
    /// </summary>
    [Display(Name = "类别深度")]
    public int classLayer { get; set; } = 1;

    /// <summary>
    /// 时间描述 多少天前
    /// </summary>
    [Display(Name = "时间描述")]
    public string? dateDescription { get; set; }

    /// <summary>
    /// 文章信息
    /// </summary>
    public ArticlesDto? article { get; set; }

    /// <summary>
    /// 子类列表
    /// </summary>
    public List<ArticleCommentDto> children { get; set; } = new List<ArticleCommentDto>();
}

/// <summary>
/// 文章评论(编辑)
/// </summary>
public class ArticleCommentAddDto
{
    /// <summary>
    /// 所属站点
    /// </summary>
    [Display(Name = "所属站点")]
    public int SiteId { get; set; }

    /// <summary>
    /// 所属文章
    /// </summary>
    [Display(Name = "所属文章")]
    public long ArticleId { get; set; }

    /// <summary>
    /// 所属父类
    /// </summary>
    [Display(Name = "所属父类")]
    public long ParentId { get; set; }

    /// <summary>
    /// 用户IP
    /// </summary>
    [Display(Name = "用户IP")]
    [StringLength(128)]
    public string? UserIp { get; set; }

    /// <summary>
    /// 评论内容
    /// </summary>
    [Display(Name = "评论内容")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(512, ErrorMessage = "{0}最多{1}位字符")]
    public string? Content { get; set; }

}
