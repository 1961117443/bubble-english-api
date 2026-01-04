using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Login;

/// <summary>
/// 会员注册
/// </summary>
public class RegisterDto : VerifyCode
{
    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    public int SiteId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [MinLength(3, ErrorMessage = "{0}至少{1}位字符")]
    [MaxLength(128, ErrorMessage = "{0}最多{2}位字符")]
    public string? UserName { get; set; }

    /// <summary>
    /// 邮箱地址
    /// </summary>
    [Display(Name = "邮箱地址")]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    [Display(Name = "手机号码")]
    [RegularExpression(@"^(13|14|15|16|18|19|17)\d{9}$")]
    public string? Phone { get; set; }

    /// <summary>
    /// 登录密码
    /// </summary>
    [Display(Name = "登录密码")]
    [MinLength(6, ErrorMessage = "{0}至少{1}位字符")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    /// <summary>
    /// 注册方式
    /// 0用户名密码注册
    /// 1手机验证码注册
    /// 2邮箱验证码注册
    /// </summary>
    [Display(Name = "注册方式")]
    [Required(ErrorMessage = "{0}不可为空")]
    public byte Method { get; set; } = 0;
}
