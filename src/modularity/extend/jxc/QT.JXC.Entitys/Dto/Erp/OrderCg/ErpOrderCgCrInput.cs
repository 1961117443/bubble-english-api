using QT.JXC.Entitys.Dto.Erp;

namespace QT.JXC.Entitys.Dto.Erp.OrderCg;

/// <summary>
/// 订单信息修改输入参数.
/// </summary>
public class ErpOrderCgCrInput
{
    /// <summary>
    /// 送货人.
    /// </summary>
    public string deliveryManId { get; set; }

    /// <summary>
    /// 送货车辆.
    /// </summary>
    public string deliveryCar { get; set; }

    /// <summary>
    /// 订单商品表.
    /// </summary>
    public List<ErpOrderdetailCrInput> erpOrderdetailList { get; set; }

    ///// <summary>
    ///// 订单处理记录表.
    ///// </summary>
    //public List<ErpOrderoperaterecordCrInput> erpOrderoperaterecordList { get; set; }

    /// <summary>
    /// 订单备注记录.
    /// </summary>
    public List<ErpOrderremarksCrInput> erpOrderremarksList { get; set; }

}