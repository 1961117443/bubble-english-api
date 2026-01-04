using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Member;

/// <summary>
/// 积分记录(显示)
/// </summary>
public class MemberPointLogDto : MemberPointLogEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [StringLength(128)]
    public string? userName { get; set; }
}

/// <summary>
/// 积分记录(增改)
/// </summary>
public class MemberPointLogEditDto
{
    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    [Required(ErrorMessage = "{0}不可为空")]
    public string userId { get; set; }

    /// <summary>
    /// 增减积分
    /// </summary>
    [Display(Name = "增减积分")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int value { get; set; } = 0;

    /// <summary>
    /// 备注说明
    /// </summary>
    [Display(Name = "备注说明")]
    [StringLength(512)]
    public string? remark { get; set; }
}
