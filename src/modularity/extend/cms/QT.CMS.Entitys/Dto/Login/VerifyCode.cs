using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QT.CMS.Entitys.Dto.Login;

/// <summary>
/// 验证码
/// </summary>
public class VerifyCode
{
    /// <summary>
    /// 验证码密钥
    /// </summary>
    [Display(Name = "验证密钥")]
    [Required(ErrorMessage = "{0}不能为空")]
    public string? CodeKey { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    [Display(Name = "验证码")]
    [Required(ErrorMessage = "{0}不能为空")]
    [MinLength(4, ErrorMessage = "{0}至少{1}位字符")]
    public string? CodeValue { get; set; }
}
