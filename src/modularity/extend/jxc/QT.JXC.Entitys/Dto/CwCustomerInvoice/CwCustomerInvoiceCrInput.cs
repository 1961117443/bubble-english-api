namespace QT.JXC.Entitys.Dto.CwCustomerInvoice;

/// <summary>
/// 收款单修改输入参数.
/// </summary>
public class CwCustomerInvoiceCrInput
{
    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 客户id.
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 收款日期.
    /// </summary>
    public DateTime? receiptDate { get; set; }

    /// <summary>
    /// 收款方式.
    /// </summary>
    public string paymentMethod { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 收款单明细.
    /// </summary>
    public List<CwCustomerInvoiceDetailCrInput> cwCustomerInvoiceDetailList { get; set; }

}