using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Login;

/// <summary>
/// 手机验证码登录
/// </summary>
public class LoginPhoneDto : VerifyCode
{
    /// <summary>
    /// 手机号码
    /// </summary>
    [Display(Name = "手机号码")]
    [Required(ErrorMessage = "{0}不能为空")]
    [RegularExpression(@"^(13|14|15|16|18|19|17)\d{9}$")]
    public string? Phone { get; set; }
}
