using QT.Common.Filter;
using QT.JXC.Entitys.Enums;

namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 订单信息列表查询输入
/// </summary>
public class ErpOrderCwListQueryInput : PageInputBase
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
    /// 公司ID.
    /// </summary>
    public string oid { get; set; }

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
    public string createTime { get; set; }

    /// <summary>
    /// 客户ID.
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? beginDate { get; set; }


    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? endDate { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public List<OrderStateEnum> states { get; set; }
}
