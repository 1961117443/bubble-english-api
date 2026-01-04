using QT.DataValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics.Entitys.Dto.LogPCWeb;

/// <summary>
/// 用户注册入参
/// </summary>
public class RegisterCrInput
{
    /// <summary>
    /// 手机号码
    /// </summary>
    [Required(ErrorMessage ="手机号码不能为空！")]
    [DataValidation(ValidationTypes.PhoneNumber)]
    public string mobile { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    [Required]
    public string code { get; set; }

    /// <summary>
    /// 用户密码
    /// </summary>
    [Required]
    public string password { get; set; }

    /// <summary>
    /// 用户类型（1：会员）
    /// </summary>
    public string userType { get; set; }
}
