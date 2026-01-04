using QT.Common.Const;
using QT.Systems.Entitys.Permission;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 会员信息
/// </summary>
[SugarTable("cms_members")]
[Tenant(ClaimConst.TENANTID)]
public class Members
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true,IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属站点
    /// </summary>
    [Display(Name = "所属站点")]
    public int SiteId { get; set; }

    /// <summary>
    /// 所属用户
    /// </summary>
    [Display(Name = "所属用户")]
    [ForeignKey("User")]
    public string? UserId { get; set; }

    /// <summary>
    /// 会员组
    /// </summary>
    [Display(Name = "会员组")]
    [ForeignKey("Group")]
    public int GroupId { get; set; }

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
    /// 性别
    /// </summary>
    [Display(Name = "性别")]
    [StringLength(30)]
    public string? Sex { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    [Display(Name = "生日")]
    public DateTime? Birthday { get; set; }

    /// <summary>
    /// 余额
    /// </summary>
    [Display(Name = "余额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; } = 0;

    /// <summary>
    /// 积分
    /// </summary>
    [Display(Name = "积分")]
    public int Point { get; set; } = 0;

    /// <summary>
    /// 经验值
    /// </summary>
    [Display(Name = "经验值")]
    public int Exp { get; set; } = 0;

    /// <summary>
    /// 注册IP
    /// </summary>
    [Display(Name = "注册IP")]
    [StringLength(128)]
    public string? RegIp { get; set; }

    /// <summary>
    /// 注册时间
    /// </summary>
    [Display(Name = "注册时间")]
    public DateTime RegTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 最后登录IP
    /// </summary>
    [Display(Name = "最后登录IP")]
    [StringLength(128)]
    public string? LastIp { get; set; }

    /// <summary>
    /// LastTime
    /// </summary>
    [Display(Name = "最后登录时间")]
    public DateTime? LastTime { get; set; }

    /// <summary>
    /// 用户信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public UserEntity? User { get; set; }

    /// <summary>
    /// 会员组
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(GroupId))]
    public MemberGroup? Group { get; set; }
}
