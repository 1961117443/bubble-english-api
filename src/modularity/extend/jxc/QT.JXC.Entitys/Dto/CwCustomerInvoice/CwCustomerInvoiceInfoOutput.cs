namespace QT.JXC.Entitys.Dto.CwCustomerInvoice;

/// <summary>
/// 收款单输出参数.
/// </summary>
public class CwCustomerInvoiceInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

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
    public List<CwCustomerInvoiceDetailInfoOutput> cwCustomerInvoiceDetailList { get; set; }

    /// <summary>
    /// 客户名称.
    /// </summary>
    public string cidName { get; set; }


    /// <summary>
    /// 公司id.
    /// </summary>
    public string oid { get; set; }

}