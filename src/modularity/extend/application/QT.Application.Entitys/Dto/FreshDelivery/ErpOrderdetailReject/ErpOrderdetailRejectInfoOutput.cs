
namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderdetailReject;

/// <summary>
/// 订单退货输出参数.
/// </summary>
public class ErpOrderdetailRejectInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

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