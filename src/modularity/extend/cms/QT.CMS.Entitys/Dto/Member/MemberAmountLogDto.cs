using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Member;
/// <summary>
/// 余额记录(显示)
/// </summary>
public class MemberAmountLogDto : MemberAmountLogEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public string id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [StringLength(128)]
    public string? userName { get; set; }
}

/// <summary>
/// 余额记录(增改)
/// </summary>
public class MemberAmountLogEditDto
{
    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string userId { get; set; }

    /// <summary>
    /// 金额
    /// </summary>
    [Display(Name = "金额")]
    [Required(ErrorMessage = "{0}不可为空")]
    public decimal value { get; set; } = 0;

    /// <summary>
    /// 备注说明
    /// </summary>
    [Display(Name = "备注说明")]
    [StringLength(512)]
    [Required(ErrorMessage = "{0}不可为空")]
    public string? remark { get; set; }
}
