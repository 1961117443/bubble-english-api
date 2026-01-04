using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.OAuth.Dto;

/// <summary>
/// 登录输入参数.
/// </summary>
[SuppressSniffer]
public class LoginInput
{
    /// <summary>
    /// 用户名.
    /// </summary>
    /// <example>13459475357</example>
    [Required(ErrorMessage = "用户名不能为空")]
    public string account { get; set; }

    /// <summary>
    /// 密码.
    /// </summary>
    /// <example>e10adc3949ba59abbe56e057f20f883e</example>
    [Required(ErrorMessage = "密码不能为空")]
    public string password { get; set; }

    /// <summary>
    /// 验证码.
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// 验证码时间戳.
    /// </summary>
    public string timestamp { get; set; }

    /// <summary>
    /// 判断是否需要验证码.
    /// </summary>
    /// <example>password</example>
    public string origin { get; set; }


    /// <summary>
    /// 来源用户
    /// </summary>
    public string sid { get; set; }
}