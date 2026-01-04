using QT.Common.Filter;
using System.Security.AccessControl;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseOutorder;

/// <summary>
/// 商家商品出库表列表查询输入
/// </summary>
public class LogEnterpriseOutorderListQueryInput : PageInputBase
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
    /// 出库订单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 出库类型.
    /// </summary>
    public string outType { get; set; }

    /// <summary>
    /// 出库日期.
    /// </summary>
    public string outTime { get; set; }

}