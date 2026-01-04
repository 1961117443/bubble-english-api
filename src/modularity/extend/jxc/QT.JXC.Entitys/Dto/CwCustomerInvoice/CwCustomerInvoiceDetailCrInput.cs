namespace QT.JXC.Entitys.Dto.CwCustomerInvoice;

/// <summary>
/// 收款单明细修改输入参数.
/// </summary>
public class CwCustomerInvoiceDetailCrInput
{
    /// <summary>
    /// 转入id.
    /// </summary>
    public string inId { get; set; }
    /// <summary>
    /// 转入类型.
    /// </summary>
    public string inType { get; set; }

    /// <summary>
    /// 收款金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

}