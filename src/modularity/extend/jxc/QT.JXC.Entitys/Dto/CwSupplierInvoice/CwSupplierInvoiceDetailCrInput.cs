namespace QT.JXC.Entitys.Dto.CwSupplierInvoice;

/// <summary>
/// 付款单明细修改输入参数.
/// </summary>
public class CwSupplierInvoiceDetailCrInput
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
    /// 付款金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

}