namespace QT.Application.Entitys.Dto.SmsCustomer;

/// <summary>
/// 客户信息修改输入参数.
/// </summary>
public class SmsCustomerCrInput
{
    /// <summary>
    /// 客户名称.
    /// </summary>
    public string customerName { get; set; }

    /// <summary>
    /// 手机号码.
    /// </summary>
    public string contactTel { get; set; }

    /// <summary>
    /// 客户地址.
    /// </summary>
    public string address { get; set; }
}