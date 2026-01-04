using QT.JXC.Entitys.Enums;

namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 订单信息输入参数.
/// </summary>
public class ErpOrderListOutput
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
    /// 客户名称.
    /// </summary>
    public string cidName { get; set; }

    /// <summary>
    /// 约定送货时间.
    /// </summary>
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 代下单人员.
    /// </summary>
    public string createUidName { get; set; }

    /// <summary>
    /// 订单金额
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 订单状态
    /// </summary>
    public OrderStateEnum state { get; set; }


    /// <summary>
    /// 订单时间.
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 送货车辆
    /// </summary>
    public string deliveryCar { get; set; }

    /// <summary>
    /// 餐别
    /// </summary>
    public string diningType { get; set; }

    /// <summary>
    /// 星期几
    /// </summary>
    public string dayOfWeek { get; set; }

    /// <summary>
    /// 所属公司
    /// </summary>
    public string oidName { get; set; }

    /// <summary>
    /// 下单时间 星期几
    /// </summary>
    public string createDayOfWeek { get; set; }

    /// <summary>
    /// 汇总明细的退货金额
    /// </summary>
    public decimal? rejectAmount { get; set; }
}

/// <summary>
/// 子单对象
/// </summary>
public class ErpSubOrderListOutput: ErpOrderListOutput
{
    /// <summary>
    /// 送货人
    /// </summary>
    public string deliveryManIdName { get; set; }
    /// <summary>
    /// 子单商品信息
    /// </summary>
    public List<ErpOrderdetailInfoOutput> erpOrderdetailList { get; set; }
}