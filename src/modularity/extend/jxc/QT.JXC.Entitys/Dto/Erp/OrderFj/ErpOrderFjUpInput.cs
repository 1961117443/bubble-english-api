namespace QT.JXC.Entitys.Dto.Erp.OrderFj;

/// <summary>
/// 订单信息更新输入.
/// </summary>
public class ErpOrderFjUpInput : ErpOrderFjCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }
}


/// <summary>
/// 更新明细的分拣信息
/// </summary>
public class ErpOrderDetailFjHlInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 分拣数量.
    /// </summary>
    public decimal num1 { get; set; }
}