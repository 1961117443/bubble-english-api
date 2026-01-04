using QT.JXC.Entitys.Enums;

namespace QT.JXC.Entitys.Dto.Erp.OrderKh;

/// <summary>
/// 订单信息输入参数.
/// </summary>
public class ErpOrderKhListOutput
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
    /// 约定送货时间.
    /// </summary>
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 订单状态
    /// </summary>
    public OrderStateEnum state { get; set; }


    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? creatorTime { get; set; }


    /// <summary>
    /// 下单时间.
    /// </summary>
    public DateTime? createTime { get; set; }


    /// <summary>
    /// 餐别.
    /// </summary>
    public string diningType { get; set; }

    /// <summary>
    /// 星期几
    /// </summary>
    public string dayOfWeek { get; set; }
}