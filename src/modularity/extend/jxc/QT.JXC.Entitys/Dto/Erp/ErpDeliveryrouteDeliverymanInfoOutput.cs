namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 线路送货员（中间表）输出参数.
/// </summary>
public class ErpDeliveryrouteDeliverymanInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 线路ID.
    /// </summary>
    public string did { get; set; }

    /// <summary>
    /// 配送员id
    /// </summary>
    public string mid { get; set; }

    /// <summary>
    /// 分拣员
    /// </summary>
    public ErpDeliverymanListOutput erpDeliveryman { get; set; }
}