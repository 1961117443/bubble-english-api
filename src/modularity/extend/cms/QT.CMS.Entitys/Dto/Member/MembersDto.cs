using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Member;

/// <summary>
/// 会员(显示)
/// </summary>
public class MembersDto : MembersEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public string id { get; set; }

    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    public string userId { get; set; }

    /// <summary>
    /// 会员组名
    /// </summary>
    [Display(Name = "会员组名")]
    public string? groupTitle { get; set; }

    /// <summary>
    /// 余额
    /// </summary>
    [Display(Name = "余额")]
    public decimal amount { get; set; }

    /// <summary>
    /// 积分
    /// </summary>
    [Display(Name = "积分")]
    public int point { get; set; }

    /// <summary>
    /// 经验值
    /// </summary>
    [Display(Name = "经验值")]
    public int exp { get; set; }

    /// <summary>
    /// 注册IP
    /// </summary>
    [Display(Name = "注册IP")]
    [MaxLength(128)]
    public string? regIp { get; set; }

    /// <summary>
    /// 注册时间
    /// </summary>
    [Display(Name = "注册时间")]
    public DateTime regTime { get; set; }

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
/// 会员(编辑)
/// </summary>
public class MembersEditDto
{
    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    public int siteId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [MinLength(3, ErrorMessage ="{0}至少{1}位字符")]
    [MaxLength(128, ErrorMessage ="{0}最多{2}位字符")]
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
    [MinLength(6, ErrorMessage = "{0}至少{1}位字符")]
    [DataType(DataType.Password)]
    public string? password { get; set; }

    /// <summary>
    /// 状态(0正常1待验证2待审核3锁定)
    /// </summary>
    [Display(Name = "账户状态")]
    public byte status { get; set; } = 0;

    /// <summary>
    /// 会员组
    /// </summary>
    [Display(Name = "会员组")]
    public int groupId { get; set; }

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
    /// 性别
    /// </summary>
    [Display(Name = "性别")]
    [MaxLength(30)]
    public string? sex { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    [Display(Name = "生日")]
    public DateTime? birthday { get; set; }
}

/// <summary>
/// 会员(修改)
/// </summary>
public class MembersModifyDto
{
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
    /// 性别
    /// </summary>
    [Display(Name = "性别")]
    [MaxLength(30)]
    public string? sex { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    [Display(Name = "生日")]
    public DateTime? birthday { get; set; }
}

/// <summary>
/// 会员统计DTO
/// </summary>
public class MembersReportDto
{
    /// <summary>
    /// 显示标题
    /// </summary>
    public string? title { get; set; }

    /// <summary>
    /// 显示数量
    /// </summary>
    public int count { get; set; }
}
