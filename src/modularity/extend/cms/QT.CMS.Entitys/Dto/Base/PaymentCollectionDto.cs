using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys.Dto.Base;

/// <summary>
/// 支付收款单(显示)
/// </summary>
public class PaymentCollectionDto : PaymentCollectionEditDto
{
    /// <summary>
    /// 所属用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    public string userId { get; set; }

    /// <summary>
    /// 交易单号
    /// </summary>
    [Display(Name = "交易单号")]
    [Required]
    [StringLength(128)]
    public string? tradeNo { get; set; }

    /// <summary>
    /// 交易类型
    /// 0商品1充值
    /// </summary>
    [Display(Name = "交易类型")]
    public byte tradeType { get; set; } = 0;

    /// <summary>
    /// 支付类型
    /// 0.在线支付
    /// 1.货到付款
    /// </summary>
    [Display(Name = "收款类型")]
    public byte paymentType { get; set; } = 0;

    /// <summary>
    /// 支付方式名称
    /// </summary>
    [Display(Name = "支付名称")]
    [StringLength(128)]
    public string? paymentTitle { get; set; }

    /// <summary>
    /// 支付金额
    /// </summary>
    [Display(Name = "支付金额")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal paymentAmount { get; set; } = 0;

    /// <summary>
    /// 收款状态
    /// 1.待支付
    /// 2.已支付
    /// 3.已取消
    /// </summary>
    [Display(Name = "收款状态")]
    public byte status { get; set; } = 1;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 开始时间
    /// </summary>
    [Display(Name = "开始时间")]
    public DateTime startTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 失效时间
    /// </summary>
    [Display(Name = "失效时间")]
    public DateTime? endTime { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    [Display(Name = "完成时间")]
    public DateTime? completeTime { get; set; }
}

public class PaymentCollectionEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long id { get; set; }

    /// <summary>
    /// 支付方式ID
    /// </summary>
    [Display(Name = "支付方式")]
    public int paymentId { get; set; }
}
