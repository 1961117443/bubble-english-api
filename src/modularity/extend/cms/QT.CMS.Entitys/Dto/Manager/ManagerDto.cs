using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Application;

/// <summary>
/// 管理员信息(显示)
/// </summary>
public class ManagerDto : ManagerEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public int id { get; set; }

    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    public string userId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 最后登录IP
    /// </summary>
    [Display(Name = "最后登录IP")]
    [MaxLength(128)]
    public string? lastIp { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    [Display(Name = "最后登录时间")]
    public DateTime? lastTime { get; set; }
}

/// <summary>
/// 管理员信息(编辑)
/// </summary>
public class ManagerEditDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [MaxLength(128)]
    public string? userName { get; set; }

    /// <summary>
    /// 邮箱地址
    /// </summary>
    [Display(Name = "邮箱地址")]
    [EmailAddress]
    public string? email { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    [Display(Name = "手机号码")]
    [RegularExpression(@"^(13|14|15|16|18|19|17)\d{9}$")]
    public string? phone { get; set; }

    /// <summary>
    /// 登录密码
    /// </summary>
    [Display(Name = "登录密码")]
    [DataType(DataType.Password)]
    public string? password { get; set; }

    /// <summary>
    /// 用户角色ID
    /// </summary>
    [Display(Name = "用户角色")]
    public int roleId { get; set; }

    /// <summary>
    /// 状态(0正常1待验证2待审核3锁定)
    /// </summary>
    [Display(Name = "账户状态")]
    public byte status { get; set; } = 0;

    /// <summary>
    /// 会员头像
    /// </summary>
    [Display(Name = "会员头像")]
    [MaxLength(512)]
    public string? avatar { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    [Display(Name = "姓名")]
    [MaxLength(30)]
    public string? realName { get; set; }

    /// <summary>
    /// 启用发布审核
    /// </summary>
    [Display(Name = "启用发布审核")]
    public byte isAudit { get; set; } = 0;
}
