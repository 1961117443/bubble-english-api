using QT.JXC.Entitys.Dto.Erp;
using QT.JXC.Entitys.Enums;

namespace QT.JXC.Entitys.Dto.Erp.OrderFj;

/// <summary>
/// 订单信息输出参数.
/// </summary>
public class ErpOrderFjInfoOutput
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
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTime? createTime { get; set; }
    /// <summary>
    /// 客户ID.
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 客户ID.
    /// </summary>
    public string cidName { get; set; }


    /// <summary>
    /// 客户类型.
    /// </summary>
    public string cidType { get; set; }

    /// <summary>
    /// 约定送货时间.
    /// </summary>
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 订单状态.
    /// </summary>
    public OrderStateEnum state { get; set; }

    /// <summary>
    /// 订单商品表.
    /// </summary>
    public List<ErpOrderdetailInfoOutput> erpOrderdetailList { get; set; }

    /// <summary>
    /// 订单处理记录表.
    /// </summary>
    public List<ErpOrderoperaterecordInfoOutput> erpOrderoperaterecordList { get; set; }

    /// <summary>
    /// 订单备注记录.
    /// </summary>
    public List<ErpOrderremarksInfoOutput> erpOrderremarksList { get; set; }

    /// <summary>
    /// 星期几
    /// </summary>
    public string dayOfWeek { get; set; }
}