namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 配送路线输出参数.
/// </summary>
public class ErpDeliveryrouteInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 简称.
    /// </summary>
    public string sortName { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 路线客户（中间表）.
    /// </summary>
    public List<ErpDeliveryCustomerInfoOutput> erpDeliveryCustomerList { get; set; }

    /// <summary>
    /// 线路送货员（中间表）.
    /// </summary>
    public List<ErpDeliveryrouteDeliverymanInfoOutput> erpDeliveryrouteDeliverymanList { get; set; }

    /// <summary>
    /// 线路分拣员（中间表）.
    /// </summary>
    public List<ErpDeliveryrouteSorterInfoOutput> erpDeliveryrouteSorterList { get; set; }

    /// <summary>
    /// 仓库覆盖路线(中间表）.
    /// </summary>
    public List<ErpStoreDeliveryInfoOutput> erpStoreDeliveryList { get; set; }

}