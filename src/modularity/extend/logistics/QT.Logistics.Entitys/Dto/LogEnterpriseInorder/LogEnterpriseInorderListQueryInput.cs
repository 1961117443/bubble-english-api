using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseInorder;

/// <summary>
/// 商家入库订单表列表查询输入
/// </summary>
public class LogEnterpriseInorderListQueryInput : PageInputBase
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
    /// 入库订单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 入库类型.
    /// </summary>
    public string inType { get; set; }

    /// <summary>
    /// 入库日期.
    /// </summary>
    public string inTime { get; set; }

}