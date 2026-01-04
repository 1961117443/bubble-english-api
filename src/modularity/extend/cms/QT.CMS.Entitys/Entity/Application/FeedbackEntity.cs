using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 留言反馈
/// </summary>
[SugarTable("cms_app_feedback")]
[Tenant(ClaimConst.TENANTID)]
public class FeedbackEntity
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true)]
    public int Id { get; set; }

    /// <summary>
    /// 所属站点
    /// </summary>
    [Display(Name = "所属站点")]
    [Required]
    public int SiteId { get; set; }

    /// <summary>
    /// 留言内容
    /// </summary>
    [Display(Name = "留言内容")]
    [Required]
    public string? Content { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(128)]
    public string? AddBy { get; set; }

    /// <summary>
    /// 留言时间
    /// </summary>
    [Display(Name = "留言时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 回复内容
    /// </summary>
    [Display(Name = "回复内容")]
    public string? ReplyContent { get; set; }

    /// <summary>
    /// 回复人
    /// </summary>
    [Display(Name = "回复人")]
    [StringLength(128)]
    public string? ReplyBy { get; set; }

    /// <summary>
    /// 回复时间
    /// </summary>
    [Display(Name = "回复时间")]
    public DateTime? ReplyTime { get; set; }

    /// <summary>
    /// 状态0未审核1正常
    /// </summary>
    [Display(Name = "状态")]
    public byte Status { get; set; } = 0;
}
