using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Article;

/// <summary>
/// 文章实体
/// </summary>
public class ArticlesBaseDto
{
    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    public int? siteId { get; set; }

    /// <summary>
    /// 所属频道ID
    /// </summary>
    [Display(Name = "所属频道")]
    public int channelId { get; set; }

    /// <summary>
    /// 调用别名
    /// </summary>
    [Display(Name = "调用别名")]
    [StringLength(128)]
    public string? callIndex { get; set; }

    /// <summary>
    /// 标题
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
    /// 外部链接
    /// </summary>
    [Display(Name = "外部链接")]
    [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
    public string? linkUrl { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    [Display(Name = "图片地址")]
    [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
    public string? imgUrl { get; set; }

    /// <summary>
    /// SEO标题
    /// </summary>
    [Display(Name = "SEO标题")]
    [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
    public string? seoTitle { get; set; }

    /// <summary>
    /// SEO关健字
    /// </summary>
    [Display(Name = "SEO关健字")]
    [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
    public string? seoKeyword { get; set; }

    /// <summary>
    /// SEO描述
    /// </summary>
    [Display(Name = "SEO描述")]
    [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
    public string? seoDescription { get; set; }

    /// <summary>
    /// 内容摘要
    /// </summary>
    [Display(Name = "内容摘要")]
    [MaxLength(255, ErrorMessage = "{0}不可超出{1}字符")]
    public string? zhaiyao { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "内容")]
    public string? content { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;

    /// <summary>
    /// 浏览总数
    /// </summary>
    [Display(Name = "浏览总数")]
    public int click { get; set; } = 0;

    /// <summary>
    /// 会员组列表
    /// </summary>
    [Display(Name = "会员组列表")]
    [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
    public string? groupIds { get; set; }

    /// <summary>
    /// 状态0正常1待审2已删
    /// </summary>
    [Display(Name = "状态")]
    public byte status { get; set; } = 0;

    /// <summary>
    /// 评论0禁止1允许
    /// </summary>
    [Display(Name = "是否评论")]
    public byte isComment { get; set; } = 0;

    /// <summary>
    /// 评论总数
    /// </summary>
    [Display(Name = "评论总数")]
    public int commentCount { get; set; } = 0;

    /// <summary>
    /// 点赞总数
    /// </summary>
    [Display(Name = "点赞总数")]
    public int likeCount { get; set; } = 0;

    /// <summary>
    /// 扩展字段键值
    /// </summary>
    [Display(Name = "扩展字段键值")]
    public Dictionary<string, string>? fields { get; set; }

}

/// <summary>
/// 文章实体(List)
/// </summary>
public class ArticlesDto : ArticlesBaseDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    public string? addBy { get; set; }

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
    /// 选中状态
    /// </summary>
    [Display(Name = "选中状态")]
    public bool @checked { get; set; } = false;

    /// <summary>
    /// 类别标题，以逗号分隔
    /// </summary>
    public string? categoryTitle { get; set; }

    /// <summary>
    /// 标签标题，以逗号分隔
    /// </summary>
    public string? labelTitle { get; set; }

    /// <summary>
    /// 扩展字段列表
    /// </summary>
    public ICollection<ArticleFieldValueDto> articleFields { get; set; } = new List<ArticleFieldValueDto>();

    /// <summary>
    /// 类别关联列表
    /// </summary>
    public ICollection<ArticleCategoryRelationDto> categoryRelations { get; set; } = new List<ArticleCategoryRelationDto>();

    /// <summary>
    /// 标签关联列表
    /// </summary>
    public ICollection<ArticleLabelRelationDto> labelRelations { get; set; } = new List<ArticleLabelRelationDto>();

    /// <summary>
    /// 相册列表
    /// </summary>
    public ICollection<ArticleAlbumDto> articleAlbums { get; set; } = new List<ArticleAlbumDto>();

    /// <summary>
    /// 附件列表
    /// </summary>
    public ICollection<ArticleAttachDto> articleAttachs { get; set; } = new List<ArticleAttachDto>();
}

/// <summary>
/// 文章实体(Add)
/// </summary>
public class ArticlesAddDto : ArticlesBaseDto
{
    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    public string? AddBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime? AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 扩展字段列表
    /// </summary>
    public ICollection<ArticleFieldValueEditDto> ArticleFields { get; set; } = new List<ArticleFieldValueEditDto>();

    /// <summary>
    /// 类别关联列表
    /// </summary>
    public ICollection<ArticleCategoryRelationDto> CategoryRelations { get; set; } = new List<ArticleCategoryRelationDto>();

    /// <summary>
    /// 标签关联列表
    /// </summary>
    public ICollection<ArticleLabelRelationDto> LabelRelations { get; set; } = new List<ArticleLabelRelationDto>();

    /// <summary>
    /// 相册列表
    /// </summary>
    public ICollection<ArticleAlbumDto> ArticleAlbums { get; set; } = new List<ArticleAlbumDto>();

    /// <summary>
    /// 附件列表
    /// </summary>
    public ICollection<ArticleAttachDto> ArticleAttachs { get; set; } = new List<ArticleAttachDto>();
}

/// <summary>
/// 文章实体(Edit)
/// </summary>
public class ArticlesEditDto : ArticlesBaseDto
{
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
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime? AddTime { get; set; }

    /// <summary>
    /// 扩展字段列表
    /// </summary>
    public ICollection<ArticleFieldValueEditDto> ArticleFields { get; set; } = new List<ArticleFieldValueEditDto>();

    /// <summary>
    /// 类别关联列表
    /// </summary>
    public ICollection<ArticleCategoryRelationDto> CategoryRelations { get; set; } = new List<ArticleCategoryRelationDto>();

    /// <summary>
    /// 标签关联列表
    /// </summary>
    public ICollection<ArticleLabelRelationDto> LabelRelations { get; set; } = new List<ArticleLabelRelationDto>();

    /// <summary>
    /// 相册列表
    /// </summary>
    public ICollection<ArticleAlbumDto> ArticleAlbums { get; set; } = new List<ArticleAlbumDto>();

    /// <summary>
    /// 附件列表
    /// </summary>
    public ICollection<ArticleAttachDto> ArticleAttachs { get; set; } = new List<ArticleAttachDto>();
}

/// <summary>
/// 文章实体(前端)
/// </summary>
public class ArticlesClientDto : ArticlesBaseDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    public string? addBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; }

    /// <summary>
    /// 类别标题，以逗号分隔
    /// </summary>
    public string? categoryTitle { get; set; }

    /// <summary>
    /// 标签标题，以逗号分隔
    /// </summary>
    public string? labelTitle { get; set; }

    /// <summary>
    /// 扩展字段列表
    /// </summary>
    public IEnumerable<ArticleFieldValueDto> articleFields { get; set; } = new List<ArticleFieldValueDto>();

    /// <summary>
    /// 类别关联列表
    /// </summary>
    public IEnumerable<ArticleCategoryRelationDto> categoryRelations { get; set; } = new List<ArticleCategoryRelationDto>();

    /// <summary>
    /// 标签关联列表
    /// </summary>
    public IEnumerable<ArticleLabelRelationDto> labelRelations { get; set; } = new List<ArticleLabelRelationDto>();

    /// <summary>
    /// 相册列表
    /// </summary>
    public IEnumerable<ArticleAlbumDto> articleAlbums { get; set; } = new List<ArticleAlbumDto>();

    /// <summary>
    /// 附件列表(前端)
    /// </summary>
    public IEnumerable<ArticleAttachClientDto> articleAttachs { get; set; } = new List<ArticleAttachClientDto>();
}

/// <summary>
/// 前端文章导航树
/// </summary>
public class ArticlesCategoryNavClientDto
{
    /// <summary>
    /// id
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 父id
    /// </summary>
    public string parentId { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    public string label { get; set; }

    /// <summary>
    /// 跳转地址
    /// </summary>
    public string href { get; set; }

    /// <summary>
    /// 子集
    /// </summary>
    public List<ArticlesCategoryNavClientDto> children { get; set; }
}