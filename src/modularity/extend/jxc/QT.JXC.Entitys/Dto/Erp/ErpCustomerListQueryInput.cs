using QT.Common.Filter;

namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 客户信息列表查询输入
/// </summary>
public class ErpCustomerListQueryInput : PageInputBase
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

    /// <summary>
    /// 客户编号.
    /// </summary>
    public string no { get; set; }


    /// <summary>
    /// 负责人.
    /// </summary>
    public string admin { get; set; }


    /// <summary>
    /// 负责人联系方式.
    /// </summary>
    public string admintel { get; set; }


    /// <summary>
    /// 送货人.
    /// </summary>
    public string deliveryManId { get; set; }

    /// <summary>
    /// 客户状态 
    /// stopOptions : [{fullName:'启用', enCode:0},{fullName:'禁用',enCode:1}],
    /// </summary>
    public int? stop { get; set; }
}