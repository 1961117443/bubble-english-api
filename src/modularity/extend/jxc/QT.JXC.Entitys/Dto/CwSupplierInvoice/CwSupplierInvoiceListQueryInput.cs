using QT.Common.Filter;

namespace QT.JXC.Entitys.Dto.CwSupplierInvoice;

/// <summary>
/// 付款单列表查询输入
/// </summary>
public class CwSupplierInvoiceListQueryInput : PageInputBase
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
    public string no { get; set; }

    /// <summary>
    /// 供应商id.
    /// </summary>
    public string sid { get; set; }

    /// <summary>
    /// 付款日期.
    /// </summary>
    public string paymentDate { get; set; }

    /// <summary>
    /// 采购日期
    /// </summary>
    public string taskBuyTimeRange { get; set; }


    /// <summary>
    /// 公司id.
    /// </summary>
    public string oid { get; set; }

}