using QT.Common.Filter;

namespace QT.JXC.Entitys.Dto;

/// <summary>
/// 收款单列表查询输入
/// </summary>
public class CwReceiptListQueryInput : PageInputBase
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
    /// 餐别
    /// </summary>
    public string diningType { get; set; }

    /// <summary>
    /// 公司id.
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 出库单号
    /// </summary>
    public string outNo { get; set; }

    /// <summary>
    /// 是否包含0
    /// </summary>
    public bool? containZero { get; set; }


    /// <summary>
    /// 发票号码
    /// </summary>
    public string fpNo { get; set; }

    /// <summary>
    /// 发票日期
    /// </summary>
    public long? fpDate { get; set; }

    /// <summary>
    /// 收款金额
    /// </summary>
    public decimal? amount { get; set; }
}