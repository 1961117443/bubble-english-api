using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 会员组
/// </summary>
[SugarTable("cms_member_group")]
[Tenant(ClaimConst.TENANTID)]
public class MemberGroup
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 组名称
    /// </summary>
    [Display(Name = "组名称")]
    [StringLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// 最小经验值
    /// </summary>
    [Display(Name = "最小经验值")]
    public int MinExp { get; set; } = 0;

    /// <summary>
    /// 最大经验值
    /// </summary>
    [Display(Name = "最大经验值")]
    public int MaxExp { get; set; } = 0;

    /// <summary>
    /// 预存款
    /// </summary>
    [Display(Name = "预存款")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; } = 0;

    /// <summary>
    /// 购物折扣
    /// </summary>
    [Display(Name = "购物折扣")]
    [Range(1, 100)]
    public int Discount { get; set; } = 100;

    /// <summary>
    /// 是否参与升级
    /// </summary>
    [Display(Name = "是否参与升级")]
    public byte IsUpgrade { get; set; } = 1;

    /// <summary>
    /// 是否默认
    /// </summary>
    [Display(Name = "是否默认")]
    public byte IsDefault { get; set; } = 0;

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(128)]
    public string? AddBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新人
    /// </summary>
    [Display(Name = "更新人")]
    [StringLength(128)]
    public string? UpdateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Display(Name = "更新时间")]
    public DateTime? UpdateTime { get; set; }
}
