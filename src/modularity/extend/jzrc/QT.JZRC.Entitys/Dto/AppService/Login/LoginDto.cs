using System.ComponentModel.DataAnnotations;

namespace QT.JZRC.Entitys.Dto.AppService.Login;


/// <summary>
/// 用户登录
/// </summary>
public class LoginDto : VerifyCode
{
    /// <summary>
    /// 用户名/手机/邮箱
    /// </summary>
    [Display(Name = "用户名/手机/邮箱")]
    [MinLength(3, ErrorMessage = "{0}至少{1}位字符")]
    [MaxLength(128, ErrorMessage = "{0}最多{2}位字符")]
    public string? UserName { get; set; }

    /// <summary>
    /// 账户密码
    /// </summary>
    [Display(Name = "账户密码")]
    [MinLength(6, ErrorMessage = "{0}至少{1}位字符")]
    [RegularExpression(@"^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{6,20}$", ErrorMessage = "{0}至少6位且是字母和数字组合")]
    public string? Password { get; set; }


    /// <summary>
    /// 登录身份（talent|company）
    /// </summary>
    [Required]
    public string LoginIdentity { get; set; }
}
