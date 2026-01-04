using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

[SugarTable("cms_member_attach_log")]
public class MemberAttachLog
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    public string UserId { get; set; }

    /// <summary>
    /// 附件ID
    /// </summary>
    [Display(Name = "附件ID")]
    [ForeignKey("ArticleAttach")]
    public long AttachId { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    [Display(Name = "文件名称")]
    [StringLength(255)]
    public string? FileName { get; set; }

    /// <summary>
    /// 下载时间
    /// </summary>
    [Display(Name = "下载时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 附件信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(AttachId))]
    public ArticleAttach? ArticleAttach { get; set; }
}
