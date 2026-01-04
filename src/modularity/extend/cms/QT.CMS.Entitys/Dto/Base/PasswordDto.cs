using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Base;

/// <summary>
/// 修改密码
/// </summary>
public class PasswordDto
{
    /// <summary>
    /// 旧密码
    /// </summary>
    [Display(Name = "旧密码")]
    [Required(ErrorMessage = "{0}不能为空")]
    [MinLength(6, ErrorMessage = "{0}至少{1}位字符")]
    [RegularExpression(@"^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{6,20}$", ErrorMessage = "{0}至少6位且是字母和数字组合")]
    public string? Password { get; set; }

    /// <summary>
    /// 新密码
    /// </summary>
    [Display(Name = "新密码")]
    [Required(ErrorMessage = "{0}不能为空")]
    [MinLength(6, ErrorMessage = "{0}至少{1}位字符")]
    [RegularExpression(@"^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{6,20}$", ErrorMessage = "{0}至少6位且是字母和数字组合")]
    public string? NewPassword { get; set; }

    /// <summary>
    /// 确认密码
    /// </summary>
    [Display(Name = "确认密码")]
    [Required(ErrorMessage = "{0}不能为空")]
    [Compare(nameof(NewPassword), ErrorMessage = "密码输入不一致")]
    public string? ConfirmPassword { get; set; }
}

/// <summary>
/// 重设密码
/// </summary>
public class PasswordResetDto : VerifyCode
{
    /// <summary>
    /// 取回方式
    /// 1手机验证码
    /// 2邮箱验证码
    /// </summary>
    [Display(Name = "取回方式")]
    [Required(ErrorMessage = "{0}不可为空")]
    public byte Method { get; set; } = 1;

    /// <summary>
    /// 手机号码
    /// </summary>
    [Display(Name = "手机号码")]
    [RegularExpression(@"^(13|14|15|16|18|19|17)\d{9}$")]
    public string? Phone { get; set; }

    /// <summary>
    /// 邮箱地址
    /// </summary>
    [Display(Name = "邮箱地址")]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// 新密码
    /// </summary>
    [Display(Name = "新密码")]
    [Required(ErrorMessage = "{0}不能为空")]
    [MinLength(6, ErrorMessage = "{0}至少{1}位字符")]
    [RegularExpression(@"^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{6,20}$", ErrorMessage = "{0}至少6位且是字母和数字组合")]
    public string? NewPassword { get; set; }

    /// <summary>
    /// 确认密码
    /// </summary>
    [Display(Name = "确认密码")]
    [Required(ErrorMessage = "{0}不能为空")]
    [Compare(nameof(NewPassword), ErrorMessage = "密码输入不一致")]
    public string? ConfirmPassword { get; set; }
}
