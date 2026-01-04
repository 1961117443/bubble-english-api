using QT.Application.Entitys.Enum.FreshDelivery;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderPs;

/// <summary>
/// 订单信息输入参数.
/// </summary>
public class ErpOrderPsListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 代下单人员.
    /// </summary>
    public string createUid { get; set; }

    /// <summary>
    /// 代下单时间.
    /// </summary>
    public DateTime? createTime { get; set; }

    /// <summary>
    /// 客户ID.
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 约定送货时间.
    /// </summary>
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 送货车辆.
    /// </summary>
    public string deliveryCar { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    public string cidName { get; set; }

    /// <summary>
    /// 订单状态
    /// </summary>
    public OrderStateEnum state { get; set; }


    /// <summary>
    /// 公司id.
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 星期几
    /// </summary>
    public string dayOfWeek { get; set; }

    /// <summary>
    /// 送货员
    /// </summary>
    public string deliveryManIdName { get; set; }
}