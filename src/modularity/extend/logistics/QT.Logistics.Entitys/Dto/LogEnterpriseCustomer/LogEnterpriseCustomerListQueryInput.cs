using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseCustomer;

/// <summary>
/// 商家客户列表查询输入
/// </summary>
public class LogEnterpriseCustomerListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

    /// <summary>
    /// 客户名称.
    /// </summary>
    public string name { get; set; }

}