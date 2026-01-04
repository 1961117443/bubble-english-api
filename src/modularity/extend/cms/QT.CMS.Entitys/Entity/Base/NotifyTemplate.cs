using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 系统通知模板
/// </summary>
[SugarTable("cms_notify_template")]
public class NotifyTemplate
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 模板类型1邮件2短信3微信
    /// </summary>
    [Display(Name = "模板类型")]
    [Range(0, 9)]
    public byte Type { get; set; } = 0;

    /// <summary>
    /// 调用名称
    /// </summary>
    [Display(Name = "调用名称")]
    [Required]
    [StringLength(128)]
    public string? CallIndex { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [StringLength(512)]
    public string? Title { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "内容")]
    [Column(TypeName = "text")]
    public string? Content { get; set; }

    /// <summary>
    /// 系统默认0否1是
    /// </summary>
    [Display(Name = "系统默认")]
    public byte IsSystem { get; set; } = 0;

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? UpdateTime { get; set; }
}
