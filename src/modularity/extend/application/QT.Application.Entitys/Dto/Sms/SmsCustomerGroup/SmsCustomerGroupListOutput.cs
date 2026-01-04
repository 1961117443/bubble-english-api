using Newtonsoft.Json;

namespace QT.Application.Entitys.Dto.SmsCustomerGroup;

/// <summary>
/// 客户信息输入参数.
/// </summary>
public class SmsCustomerGroupListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

     

    /// <summary>
    /// 分组名称.
    /// </summary>
    public string groupName { get; set; }

}