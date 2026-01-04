using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Member;

/// <summary>
/// 会员组别(显示)
/// </summary>
public class MemberGroupDto : MemberGroupEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public int id { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(128)]
    public string? addBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    [StringLength(128)]
    public string? updateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? updateTime { get; set; }
}

/// <summary>
/// 会员组别(编辑)
/// </summary>
public class MemberGroupEditDto
{
    /// <summary>
    /// 组名称
    /// </summary>
    [Display(Name = "组名称")]
    [Required(ErrorMessage = "{0}不可为空")]
    [StringLength(128)]
    public string? title { get; set; }

    /// <summary>
    /// 最小经验值
    /// </summary>
    [Display(Name = "最小经验值")]
    public int minExp { get; set; } = 0;

    /// <summary>
    /// 最大经验值
    /// </summary>
    [Display(Name = "最大经验值")]
    public int maxExp { get; set; } = 0;

    /// <summary>
    /// 预存款
    /// </summary>
    [Display(Name = "预存款")]
    public decimal amount { get; set; } = 0;

    /// <summary>
    /// 购物折扣
    /// </summary>
    [Display(Name = "购物折扣")]
    [Range(1, 100)]
    public int discount { get; set; } = 100;

    /// <summary>
    /// 是否参与升级
    /// </summary>
    [Display(Name = "是否参与升级")]
    public byte isUpgrade { get; set; } = 1;

    /// <summary>
    /// 是否默认
    /// </summary>
    [Display(Name = "是否默认")]
    public byte isDefault { get; set; } = 0;
}
