using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Article;

/// <summary>
/// 文章投稿(公共)
/// </summary>
public class ArticleContributeBaseDto
{
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
    /// 文章标题
    /// </summary>
    [Display(Name = "标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
    public string? title { get; set; }

    /// <summary>
    /// 来源
    /// </summary>
    [Display(Name = "来源")]
    [MaxLength(128, ErrorMessage = "{0}不可超出{1}字符")]
    public string? source { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    [Display(Name = "作者")]
    [MaxLength(128, ErrorMessage = "{0}不可超出{1}字符")]
    public string? author { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    [Display(Name = "图片地址")]
    [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
    public string? imgUrl { get; set; }

    /// <summary>
    /// 扩展字段集合
    /// </summary>
    [Display(Name = "扩展字段集合")]
    public string? FieldMeta { get; set; }

    /// <summary>
    /// 相册扩展字段值
    /// </summary>
    [Display(Name = "相册扩展字段值")]
    public string? albumMeta { get; set; }

    /// <summary>
    /// 附件扩展字段值
    /// </summary>
    [Display(Name = "附件扩展字段值")]
    public string? attachMeta { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "内容")]
    public string? content { get; set; }

    /// <summary>
    /// 投稿用户
    /// </summary>
    [Display(Name = "投稿用户")]
    public string userId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    public string? userName { get; set; }

    /// <summary>
    /// 状态0待审1通过2返回
    /// </summary>
    [Display(Name = "状态")]
    public byte status { get; set; } = 0;

}
/// <summary>
/// 文章投稿(显示)
/// </summary>
public class ArticleContributeDto: ArticleContributeBaseDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    public string? updateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? updateTime { get; set; }
}

/// <summary>
/// 文章投稿(添加)
/// </summary>
public class ArticleContributeAddDto : ArticleContributeBaseDto
{
    /// <summary>
    /// 扩展字段值
    /// </summary>
    public List<ArticleContributeFieldValueDto>? Fields { get; set; }

    /// <summary>
    /// 相册
    /// </summary>
    public List<ArticleAlbumDto>? Albums { get; set; }

    /// <summary>
    /// 附件
    /// </summary>
    public List<ArticleAttachDto>? Attachs { get; set; }
}

/// <summary>
/// 文章投稿(编辑)
/// </summary>
public class ArticleContributeEditDto : ArticleContributeBaseDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long Id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    public string? UpdateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 扩展字段值
    /// </summary>
    public List<ArticleContributeFieldValueDto>? Fields { get; set; }
    /// <summary>
    /// 相册
    /// </summary>
    public List<ArticleAlbumDto>? Albums { get; set; }
    /// <summary>
    /// 附件
    /// </summary>
    public List<ArticleAttachDto>? Attachs { get; set; }

    /// <summary>
    /// 所属分类
    /// </summary>
    public string[]? Categorys { get; set; }
}

/// <summary>
/// 文章投稿编辑展示(查看)
/// </summary>
public class ArticleContributeViewDto : ArticleContributeBaseDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    public string? updateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? updateTime { get; set; }

    /// <summary>
    /// 扩展字段
    /// </summary>
    public List<ArticleContributeFieldValueDto>? fields { get; set; }

    /// <summary>
    /// 相册
    /// </summary>
    public List<ArticleAlbumDto>? albums { get; set; }

    /// <summary>
    /// 附件
    /// </summary>
    public List<ArticleAttachDto>? attachs { get; set; }
}
