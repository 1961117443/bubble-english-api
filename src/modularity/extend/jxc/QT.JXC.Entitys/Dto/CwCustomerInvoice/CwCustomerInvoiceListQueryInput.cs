using QT.Common.Filter;

namespace QT.JXC.Entitys.Dto.CwCustomerInvoice;

/// <summary>
/// 收款单列表查询输入
/// </summary>
public class CwCustomerInvoiceListQueryInput : PageInputBase
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
    /// 客户id.
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 收款日期.
    /// </summary>
    public string receiptDate { get; set; }

    /// <summary>
    /// 收款方式.
    /// </summary>
    public string paymentMethod { get; set; }

    /// <summary>
    /// 订单时间范围
    /// </summary>
    public string createTime { get; set; }
    
    /// <summary>
    /// 约定配送时间范围
    /// </summary>
    public string posttime { get; set; }



    /// <summary>
    /// 餐别.
    /// </summary>
    public string diningType { get; set; }

    /// <summary>
    /// 公司id.
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 付款金额
    /// </summary>
    public decimal? amountMin { get; set; }

    /// <summary>
    /// 付款金额
    /// </summary>
    public decimal? amountMax { get; set; }
}