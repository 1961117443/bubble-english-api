namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 配送路线修改输入参数.
/// </summary>
public class ErpDeliveryrouteCrInput
{
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
    public List<ErpDeliveryCustomerCrInput> erpDeliveryCustomerList { get; set; }

    /// <summary>
    /// 线路送货员（中间表）.
    /// </summary>
    public List<ErpDeliveryrouteDeliverymanCrInput> erpDeliveryrouteDeliverymanList { get; set; }

    /// <summary>
    /// 线路分拣员（中间表）.
    /// </summary>
    public List<ErpDeliveryrouteSorterCrInput> erpDeliveryrouteSorterList { get; set; }

    /// <summary>
    /// 仓库覆盖路线(中间表）.
    /// </summary>
    public List<ErpStoreDeliveryCrInput> erpStoreDeliveryList { get; set; }

}