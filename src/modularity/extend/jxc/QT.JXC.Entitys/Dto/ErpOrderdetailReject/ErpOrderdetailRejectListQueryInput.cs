using QT.Common.Filter;

namespace QT.JXC.Entitys.Dto.ErpOrderdetailReject;

/// <summary>
/// 订单退货列表查询输入
/// </summary>
public class ErpOrderdetailRejectListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

    ///// <summary>
    ///// 退货数量.
    ///// </summary>
    //public string num { get; set; }

    /// <summary>
    /// 订单编号
    /// </summary>
    public string orderNo { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    public string cidName { get; set; }

    public DateTime? posttime { get; set; }

}

/// <summary>
/// 订单退货列表查询输入
/// </summary>
public class ErpOrderdetailRejectQueryOrderListQueryInput : PageInputBase
{

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 客户名称.
    /// </summary>
    public string customerName { get; set; }

}