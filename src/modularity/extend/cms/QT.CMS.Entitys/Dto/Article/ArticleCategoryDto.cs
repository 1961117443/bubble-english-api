using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Article;

/// <summary>
/// 文章类别(显示)
/// </summary>
public class ArticleCategoryDto : ArticleCategoryEditDto
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
    /// 子类列表
    /// </summary>
    public List<ArticleCategoryDto> children { get; set; } = new List<ArticleCategoryDto>();
}

/// <summary>
/// 文章类别(编辑)
/// </summary>
public class ArticleCategoryEditDto
{
    /// <summary>
    /// 所属频道ID
    /// </summary>
    [Display(Name = "所属频道")]
    public int channelId { get; set; }

    /// <summary>
    /// 所属父类ID
    /// </summary>
    [Display(Name = "所属父类")]
    public long parentId { get; set; } = 0;

    /// <summary>
    /// 调用别名
    /// </summary>
    [Display(Name = "调用别名")]
    [MaxLength(128, ErrorMessage = "{0}不可超出{1}字符")]
    public string? callIndex { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(128, ErrorMessage = "{0}不可超出{1}字符")]
    [MinLength(1, ErrorMessage = "{0}不可小于{1}字符")]
    public string? title { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    [Display(Name = "图片地址")]
    [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
    public string? imgUrl { get; set; }

    /// <summary>
    /// 外部链接
    /// </summary>
    [Display(Name = "外部链接")]
    [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
    public string? linkUrl { get; set; }

    /// <summary>
    /// 内容介绍
    /// </summary>
    [Display(Name = "内容介绍")]
    public string? content { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;

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
    /// 状态0正常1禁用
    /// </summary>
    [Display(Name = "状态")]
    public byte status { get; set; } = 0;
}

/// <summary>
/// 商品分类(带商品)
/// </summary>
public class ArticleCategoryClientDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    public string? title { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    [Display(Name = "图片地址")]
    public string? imgUrl { get; set; }

    /// <summary>
    /// 文章列表
    /// </summary>
    public IEnumerable<ArticlesClientDto> data { get; set; } = new List<ArticlesClientDto>();
}
