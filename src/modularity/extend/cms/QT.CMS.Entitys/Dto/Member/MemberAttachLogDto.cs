using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Member;

/// <summary>
/// 附件下载记录(显示)
/// </summary>
public class MemberAttachLogDto : MemberAttachLogEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public string id { get; set; }

    /// <summary>
    /// 下载时间
    /// </summary>
    [Display(Name = "下载时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [StringLength(128)]
    public string? userName { get; set; }
}

/// <summary>
/// 附件下载记录(增改)
/// </summary>
public class MemberAttachLogEditDto
{
    /// <summary>
    /// 所属用户
    /// </summary>
    [Display(Name = "所属用户")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string userId { get; set; }

    /// <summary>
    /// 附件ID
    /// </summary>
    [Display(Name = "附件ID")]
    [Required(ErrorMessage = "{0}不可为空")]
    public long attachId { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    [Display(Name = "文件名称")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(255)]
    public string? fileName { get; set; }
}
