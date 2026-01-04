using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics.Entitys.Dto.LogPCWeb;

/// <summary>
/// 用户登录入参
/// </summary>
public class UserLoginInput
{
    /// <summary>
    /// 账号
    /// </summary>
    [Required]
    public string userName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    public string passWord { get; set; }

    /// <summary>
    /// 用户类型（1：会员，2：配送点工作人员，3：商家）
    /// </summary>
    [Required]
    public string userType { get; set; }
}


public enum LoginUserRoleType
{
    [Display(Name ="会员")]
    Member,
    [Display(Name = "员工")]
    Worker,
    [Display(Name = "商户")]
    Merchant
}