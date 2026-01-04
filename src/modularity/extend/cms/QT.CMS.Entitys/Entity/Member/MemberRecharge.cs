using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 会员充值
/// </summary>
[SugarTable("cms_member_recharge")]
public class MemberRecharge
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 支付收款ID
    /// </summary>
    [Display(Name = "支付收款")]
    [ForeignKey("Collection")]
    public long CollectionId { get; set; }

    /// <summary>
    /// 充值用户ID
    /// </summary>
    [Display(Name = "充值用户")]
    public string UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [StringLength(128)]
    public string? UserName { get; set; }

    /// <summary>
    /// 充值金额
    /// </summary>
    [Display(Name = "充值金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; } = 0;


    /// <summary>
    /// 支付收款
    /// </summary>
    [Navigate(NavigateType.OneToOne , nameof(CollectionId))]
    public PaymentCollection? Collection { get; set; }
}
