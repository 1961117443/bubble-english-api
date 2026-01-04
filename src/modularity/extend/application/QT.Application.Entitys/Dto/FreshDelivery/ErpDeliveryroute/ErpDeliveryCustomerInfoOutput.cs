using QT.Application.Entitys.Dto.FreshDelivery.ErpCustomer;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpDeliveryCustomer;

/// <summary>
/// 路线客户（中间表）输出参数.
/// </summary>
public class ErpDeliveryCustomerInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }


    /// <summary>
    /// 路线ID.
    /// </summary>
    public string did { get; set; }

    /// <summary>
    /// 客户ID.
    /// </summary>
    public string cid { get; set; }


    public ErpCustomerListOutput erpCustomer { get; set; }
}