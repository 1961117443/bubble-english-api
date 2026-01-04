namespace QT.Logistics.Entitys.Dto.LogEnterpriseCustomer;

/// <summary>
/// 商家客户输入参数.
/// </summary>
public class LogEnterpriseCustomerListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 客户名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 客户地址.
    /// </summary>
    public string address { get; set; }

    /// <summary>
    /// 联系人.
    /// </summary>
    public string admin { get; set; }

    /// <summary>
    /// 联系人电话.
    /// </summary>
    public string admintel { get; set; }

}