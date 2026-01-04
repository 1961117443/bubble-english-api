using QT.Systems.Entitys.Permission;

namespace QT.CMS.Entitys;

/// <summary>
/// 管理员信息
/// </summary>
[SugarTable("cms_manager")]
[Tenant(ClaimConst.TENANTID)]
public class Manager
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true)]
    public int Id { get; set; }

    /// <summary>
    /// 所属用户
    /// </summary>
    [Display(Name = "所属用户")]
    [ForeignKey("User")]
    public string UserId { get; set; }

    /// <summary>
    /// 会员头像
    /// </summary>
    [Display(Name = "会员头像")]
    [StringLength(512)]
    public string? Avatar { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    [Display(Name = "姓名")]
    [StringLength(30)]
    public string? RealName { get; set; }

    /// <summary>
    /// 启用发布审核
    /// </summary>
    [Display(Name = "启用发布审核")]
    public byte IsAudit { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 最后登录IP
    /// </summary>
    [Display(Name = "最后登录IP")]
    [StringLength(128)]
    public string? LastIp { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    [Display(Name = "最后登录时间")]
    public DateTime? LastTime { get; set; }

    /// <summary>
    /// 用户信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public UserEntity? User { get; set; }
}
