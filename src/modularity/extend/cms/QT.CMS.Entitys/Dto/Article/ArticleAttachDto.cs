using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Article;

/// <summary>
/// 文章附件
/// </summary>
public class ArticleAttachDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 所属文章ID
    /// </summary>
    [Display(Name = "所属文章")]
    public long articleId { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    [Display(Name = "文件名")]
    public string? fileName { get; set; }

    /// <summary>
    /// 文件路径
    /// </summary>
    [Display(Name = "文件路径")]
    public string? filePath { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    [Display(Name = "文件大小")]
    public int fileSize { get; set; }

    /// <summary>
    /// 扩展名
    /// </summary>
    [Display(Name = "扩展名")]
    public string? fileExt { get; set; }

    /// <summary>
    /// 下载所需积分
    /// </summary>
    [Display(Name = "下载所需积分")]
    public int point { get; set; } = 0;

    /// <summary>
    /// 下载次数
    /// </summary>
    [Display(Name = "下载次数")]
    public int downCount { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 文件大小
    /// </summary>
    [Display(Name = "文件大小")]
    public string? size { get; set; }

    /// <summary>
    /// 会员权限
    /// </summary>
    [Display(Name = "会员权限")]
    public int isMember { get; set; }
}

/// <summary>
/// 文章附件(前端)
/// </summary>
public class ArticleAttachClientDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long Id { get; set; }

    /// <summary>
    /// 所属文章ID
    /// </summary>
    [Display(Name = "所属文章")]
    public long ArticleId { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    [Display(Name = "文件名")]
    public string? FileName { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    [Display(Name = "文件大小")]
    public int FileSize { get; set; }

    /// <summary>
    /// 扩展名
    /// </summary>
    [Display(Name = "扩展名")]
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
    /// 文件大小
    /// </summary>
    [Display(Name = "文件大小")]
    public string? Size { get; set; }
}
