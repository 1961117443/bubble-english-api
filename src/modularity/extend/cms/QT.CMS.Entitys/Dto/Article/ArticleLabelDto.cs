using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Article;

/// <summary>
/// 文章标签(公共)
/// </summary>
public class ArticleLabelBaseDto
{
    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    public string? title { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;

    /// <summary>
    /// 状态0正常1禁用
    /// </summary>
    [Display(Name = "状态")]
    public byte status { get; set; } = 0;

}

/// <summary>
/// 文章标签(List)
/// </summary>
public class ArticleLabelDto : ArticleLabelBaseDto
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
}

/// <summary>
/// 文章标签(Add)
/// </summary>
public class ArticleLabelAddDto : ArticleLabelBaseDto
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
    public DateTime AddTime { get; set; } = DateTime.Now;
}

/// <summary>
/// 文章标签(Edit)
/// </summary>
public class ArticleLabelEditDto : ArticleLabelBaseDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long Id { get; set; }

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
}
