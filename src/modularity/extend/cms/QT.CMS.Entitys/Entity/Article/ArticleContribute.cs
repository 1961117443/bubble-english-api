using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 文章投稿
/// </summary>
[SugarTable("cms_article_contribute")]
[Tenant(ClaimConst.TENANTID)]
public class ArticleContribute
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
    [ForeignKey("Channel")]
    public int ChannelId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required]
    [StringLength(512)]
    public string? Title { get; set; }

    /// <summary>
    /// 来源
    /// </summary>
    [Display(Name = "来源")]
    [StringLength(128)]
    public string? Source { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    [Display(Name = "作者")]
    [StringLength(128)]
    public string? Author { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    [Display(Name = "图片地址")]
    [StringLength(512)]
    public string? ImgUrl { get; set; }

    /// <summary>
    /// 扩展字段集合
    /// </summary>
    [Display(Name = "扩展字段集合")]
    public string? FieldMeta { get; set; }

    /// <summary>
    /// 相册扩展字段值
    /// </summary>
    [Display(Name = "相册扩展字段值")]
    public string? AlbumMeta { get; set; }

    /// <summary>
    /// 附件扩展字段值
    /// </summary>
    [Display(Name = "附件扩展字段值")]
    public string? AttachMeta { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "内容")]
    [Column(TypeName = "text")]
    public string? Content { get; set; }

    /// <summary>
    /// 投稿用户ID
    /// </summary>
    [Display(Name = "投稿用户")]
    public string UserId { get; set; }

    /// <summary>
    /// 投稿用户名
    /// </summary>
    [Display(Name = "用户名")]
    [StringLength(128)]
    public string? UserName { get; set; }

    /// <summary>
    /// 状态0待审1通过2返回
    /// </summary>
    [Display(Name = "状态")]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    [StringLength(128)]
    public string? UpdateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 频道信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ChannelId))]
    public SiteChannel? Channel { get; set; }
}
