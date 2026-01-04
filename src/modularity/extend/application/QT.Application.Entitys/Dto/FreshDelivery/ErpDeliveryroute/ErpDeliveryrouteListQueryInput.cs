using QT.Common.Filter;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpDeliveryroute;

/// <summary>
/// 配送路线列表查询输入
/// </summary>
public class ErpDeliveryrouteListQueryInput : PageInputBase
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
    /// 名称.
    /// </summary>
    public string name { get; set; }

}