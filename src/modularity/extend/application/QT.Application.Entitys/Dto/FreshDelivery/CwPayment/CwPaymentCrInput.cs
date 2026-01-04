using QT.Application.Entitys.Dto.FreshDelivery.CwPaymentDetail;

namespace QT.Application.Entitys.Dto.FreshDelivery.CwPayment;

/// <summary>
/// 付款单修改输入参数.
/// </summary>
public class CwPaymentCrInput
{
    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 供应商id.
    /// </summary>
    public string sid { get; set; }

    /// <summary>
    /// 付款日期.
    /// </summary>
    public DateTime? paymentDate { get; set; }

    /// <summary>
    /// 付款方式.
    /// </summary>
    public string paymentMethod { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 付款单明细.
    /// </summary>
    public List<CwPaymentDetailCrInput> cwPaymentDetailList { get; set; }

}