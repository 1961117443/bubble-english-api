using System.ComponentModel.DataAnnotations;

namespace QT.JZRC.Entitys.Dto.AppService.Login;

/// <summary>
/// 授权登录
/// </summary>
public class LoginOAuthDto
{
    /// <summary>
    /// 站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int SiteId { get; set; }

    /// <summary>
    /// 授权平台标识
    /// </summary>
    [Display(Name = "授权平台标识")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? Provider { get; set; }

    /// <summary>
    /// 接口方式
    /// web|mp|app
    /// </summary>
    [Display(Name = "授权接口")]
    public string Type { get; set; } = "web";

    /// <summary>
    /// 认证的Code
    /// </summary>
    [Display(Name = "认证Code")]
    public string? Code { get; set; }

    /// <summary>
    /// 跳转的URL
    /// </summary>
    [Display(Name = "跳转的URL")]
    public string? RedirectUri { get; set; }

    /// <summary>
    /// 用户昵称
    /// </summary>
    [Display(Name = "用户昵称")]
    public string? RealName { get; set; }

    /// <summary>
    /// 用户头像
    /// </summary>
    [Display(Name = "用户头像")]
    public string? Avatar { get; set; }
}
