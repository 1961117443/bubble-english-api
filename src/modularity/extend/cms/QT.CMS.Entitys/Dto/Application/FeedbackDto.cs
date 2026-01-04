using QT.CMS.Entitys.Dto.Login;
using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Application;

/// <summary>
/// 留言反馈(显示)
/// </summary>
public class FeedbackDto: FeedbackEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public int id { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(128)]
    public string? addBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 回复人
    /// </summary>
    [Display(Name = "回复人")]
    [StringLength(128)]
    public string? replyBy { get; set; }

    /// <summary>
    /// 回复时间
    /// </summary>
    [Display(Name = "回复时间")]
    public DateTime? replyTime { get; set; }
}

/// <summary>
/// 留言反馈(编辑)
/// </summary>
public class FeedbackEditDto
{
    /// <summary>
    /// 站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int siteId { get; set; }

    /// <summary>
    /// 留言内容
    /// </summary>
    [Display(Name = "留言内容")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? content { get; set; }

    /// <summary>
    /// 回复内容
    /// </summary>
    [Display(Name = "回复内容")]
    public string? replyContent { get; set; }

    /// <summary>
    /// 状态0未审核1正常
    /// </summary>
    [Display(Name = "状态")]
    public byte status { get; set; } = 0;
}

/// <summary>
/// 留言反馈(前端)
/// </summary>
public class FeedbackClientDto : VerifyCode
{
    /// <summary>
    /// 站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int siteId { get; set; }

    /// <summary>
    /// 留言内容
    /// </summary>
    [Display(Name = "留言内容")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? content { get; set; }
}
