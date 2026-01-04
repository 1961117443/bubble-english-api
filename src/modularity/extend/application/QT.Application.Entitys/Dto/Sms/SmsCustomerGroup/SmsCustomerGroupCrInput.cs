namespace QT.Application.Entitys.Dto.SmsCustomerGroup;

/// <summary>
/// 修改输入参数.
/// </summary>
public class SmsCustomerGroupCrInput
{
    /// <summary>
    /// 分组名称.
    /// </summary>
    public string groupName { get; set; }


    public List<SmsCustomerGroupDetailCrInput> smsCustomerGroupDetails { get; set; }
}
