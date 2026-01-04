using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys;

/// <summary>
/// 余额支付下单
/// </summary>
public class BalancePayDto
{
    /// <summary>
    /// 商户订单号
    /// </summary>
    [Display(Name = "订单号")]
    [Required(ErrorMessage = "{0}不能为空")]
    public string? OutTradeNo { get; set; }
}
