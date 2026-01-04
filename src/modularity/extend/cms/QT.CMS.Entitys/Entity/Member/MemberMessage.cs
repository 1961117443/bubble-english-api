using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 站内消息
/// </summary>
[SugarTable("cms_member_message")]
[Tenant(ClaimConst.TENANTID)]
public class MemberMessage
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 接收用户ID
    /// </summary>
    [Display(Name = "接收用户")]
    public string UserId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [StringLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "内容")]
    [Column(TypeName = "text")]
    public string? Content { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 是否已读
    /// </summary>
    [Display(Name = "是否已读")]
    public byte IsRead { get; set; } = 0;

    /// <summary>
    /// 读取时间
    /// </summary>
    [Display(Name = "读取时间")]
    public DateTime? ReadTime { get; set; }
}
