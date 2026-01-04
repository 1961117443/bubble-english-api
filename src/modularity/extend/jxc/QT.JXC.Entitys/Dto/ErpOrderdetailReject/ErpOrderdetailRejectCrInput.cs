namespace QT.JXC.Entitys.Dto.ErpOrderdetailReject;

/// <summary>
/// 订单退货修改输入参数.
/// </summary>
public class ErpOrderdetailRejectCrInput
{
    /// <summary>
    /// 订单明细id.
    /// </summary>
    public string orderDetailId { get; set; }

    /// <summary>
    /// 退货数量.
    /// </summary>
    public decimal num { get; set; }

    /// <summary>
    /// 退货金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

}