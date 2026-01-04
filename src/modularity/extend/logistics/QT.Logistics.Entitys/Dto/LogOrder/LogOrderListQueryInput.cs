using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogOrder;

/// <summary>
/// 订单管理列表查询输入
/// </summary>
public class LogOrderListQueryInput : PageInputBase
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
    /// 订单编号.
    /// </summary>
    public string orderNo { get; set; }

    /// <summary>
    /// 寄件配送点id.
    /// </summary>
    public string sendPointId { get; set; }

    /// <summary>
    /// 送达配送点id.
    /// </summary>
    public string reachPointId { get; set; }

    /// <summary>
    /// 订单日期.
    /// </summary>
    public string orderDate { get; set; }

    /// <summary>
    /// 订单状态.
    /// </summary>
    public string orderStatus { get; set; }

    /// <summary>
    /// 寄件人姓名.
    /// </summary>
    public string shipperName { get; set; }

    /// <summary>
    /// 寄件人电话.
    /// </summary>
    public string shipperPhone { get; set; }

    /// <summary>
    /// 收件人姓名.
    /// </summary>
    public string recipientName { get; set; }

    /// <summary>
    /// 收件人电话.
    /// </summary>
    public string recipientPhone { get; set; }


    /// <summary>
    /// 数据范围(scope=point 配送点)
    /// </summary>
    public string scope { get; set; }
}