namespace QT.Application.Entitys.Dto.SmsCustomerGroup;

/// <summary>
/// 客户信息输出参数.
/// </summary>
public class SmsCustomerGroupInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 客户名称.
    /// </summary>
    public string groupName { get; set; }

    public List<SmsCustomerGroupDetailInfoOutput> smsCustomerGroupDetails { get; set; }
}
