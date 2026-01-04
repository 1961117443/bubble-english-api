using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Base;

/// <summary>
/// 系统通知模板(显示)
/// </summary>
public class NotifyTemplateDto: NotifyTemplateEditDto
{
    [Display(Name = "自增ID")]
    public int id { get; set; }

    /// <summary>
    /// 系统默认0否1是
    /// </summary>
    [Display(Name = "系统默认")]
    public byte isSystem { get; set; } = 0;

    [Display(Name = "更新时间")]
    public DateTime? updateTime { get; set; }
}

/// <summary>
/// 系统通知模板(编辑)
/// </summary>
public class NotifyTemplateEditDto
{
    /// <summary>
    /// 模板类型1邮件2短信3微信
    /// </summary>
    [Display(Name = "模板类型")]
    [Range(1, 3, ErrorMessage = "{0}只能选择1-3其中一项")]
    public byte type { get; set; } = 0;

    [Display(Name = "调用名称")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(128, ErrorMessage = "{0}不可超出{1}字符")]
    public string? callIndex { get; set; }

    [Display(Name = "标题")]
    [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
    public string? title { get; set; }

    [Display(Name = "内容")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? content { get; set; }
}
