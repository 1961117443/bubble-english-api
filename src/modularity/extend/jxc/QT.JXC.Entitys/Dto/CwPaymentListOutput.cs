namespace QT.JXC.Entitys.Dto;

/// <summary>
/// 付款单输入参数.
/// </summary>
public class CwPaymentListOutput
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
    /// 付款金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 供应商.
    /// </summary>
    public string sidName { get; set; }


    /// <summary>
    /// 所属公司.
    /// </summary>
    public string oidName { get; set; }

    /// <summary>
    /// 所属公司id.
    /// </summary>
    public string oid { get; set; }
}