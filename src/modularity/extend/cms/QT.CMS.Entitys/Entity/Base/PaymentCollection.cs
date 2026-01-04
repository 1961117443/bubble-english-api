using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 支付收款
/// </summary>
[SugarTable("cms_payment_collection")]
public class PaymentCollection
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true,IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    public string? UserId { get; set; }

    /// <summary>
    /// 交易单号
    /// </summary>
    [Display(Name = "交易单号")]
    [Required]
    [StringLength(128)]
    public string? TradeNo { get; set; }

    /// <summary>
    /// 交易类型
    /// 0商品1充值
    /// </summary>
    [Display(Name = "交易类型")]
    public byte TradeType { get; set; } = 0;

    /// <summary>
    /// 支付方式ID
    /// </summary>
    [Display(Name = "支付方式")]
    [ForeignKey("SitePayment")]
    public int PaymentId { get; set; }

    /// <summary>
    /// 支付类型
    /// 0.在线支付
    /// 1.货到付款
    /// </summary>
    [Display(Name = "收款类型")]
    public byte PaymentType { get; set; } = 0;

    /// <summary>
    /// 支付方式名称
    /// </summary>
    [Display(Name = "支付名称")]
    [StringLength(128)]
    public string? PaymentTitle { get; set; }

    /// <summary>
    /// 支付金额
    /// </summary>
    [Display(Name = "支付金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal PaymentAmount { get; set; } = 0;

    /// <summary>
    /// 收款状态
    /// 1.待支付
    /// 2.已支付
    /// 3.已取消
    /// </summary>
    [Display(Name = "收款状态")]
    public byte Status { get; set; } = 1;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 开始时间
    /// </summary>
    [Display(Name = "开始时间")]
    public DateTime StartTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 失效时间
    /// </summary>
    [Display(Name = "失效时间")]
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    [Display(Name = "完成时间")]
    public DateTime? CompleteTime { get; set; }


    /// <summary>
    /// 会员充值订单列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(Entitys.MemberRecharge.CollectionId))]
    public List<MemberRecharge> Recharges { get; set; }

    // <summary>
    /// 订单收款关联列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(Entitys.Orders.CollectionId))]
    public List<Orders> Orders { get; set; }

    /// <summary>
    /// 支付方式
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(PaymentId))]
    public SitePayment? SitePayment { get; set; }
}
