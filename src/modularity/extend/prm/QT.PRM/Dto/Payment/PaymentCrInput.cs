using QT.DependencyInjection;
using QT.PRM.Entitys;
using System.ComponentModel.DataAnnotations;

namespace QT.PRM.Dto.Payment;

/// <summary>
/// 收费流水创建输入
/// </summary>
[SuppressSniffer]
public class PaymentCrInput
{
    /// <summary>
    /// 应收款ID
    /// </summary>
    [Required(ErrorMessage = "应收款ID不能为空")]
    public string receivableId { get; set; }

    /// <summary>
    /// 金额
    /// </summary>
    [Required(ErrorMessage = "金额不能为空")]
    [Range(0, double.MaxValue, ErrorMessage = "金额必须大于等于0")]
    public decimal amount { get; set; }

    /// <summary>
    /// 支付方式
    /// </summary>
    [Required(ErrorMessage = "支付方式不能为空")]
    public PaymentMethod paymentMethod { get; set; }

    /// <summary>
    /// 付款人类型
    /// </summary>
    [Required(ErrorMessage = "付款人类型不能为空")]
    public PayerType payerType { get; set; }

    /// <summary>
    /// 发票号
    /// </summary>
    [StringLength(50, ErrorMessage = "发票号不能超过50字符")]
    public string invoiceNumber { get; set; }
}
