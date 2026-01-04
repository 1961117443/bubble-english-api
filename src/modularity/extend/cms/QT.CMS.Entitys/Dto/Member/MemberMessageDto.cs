using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Member;

/// <summary>
/// 站内消息(显示)
/// </summary>
public class MemberMessageDto : MemberMessageEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public string id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [StringLength(128)]
    public string? userName { get; set; }
}

/// <summary>
/// 站内消息(增改)
/// </summary>
public class MemberMessageEditDto
{
    /// <summary>
    /// 接收用户ID
    /// </summary>
    [Display(Name = "接收用户")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string userId { get; set; }

    /// <summary>
    /// 消息标题
    /// </summary>
    [Display(Name = "标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MinLength(3, ErrorMessage = "{0}不可少于{1}字符")]
    [MaxLength(128, ErrorMessage = "{0}不可多于{2}字符")]
    public string? title { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "内容")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? content { get; set; }

    /// <summary>
    /// 是否已读
    /// </summary>
    [Display(Name = "是否已读")]
    public byte isRead { get; set; } = 0;

    /// <summary>
    /// 读取时间
    /// </summary>
    [Display(Name = "读取时间")]
    public DateTime? readTime { get; set; }
}
