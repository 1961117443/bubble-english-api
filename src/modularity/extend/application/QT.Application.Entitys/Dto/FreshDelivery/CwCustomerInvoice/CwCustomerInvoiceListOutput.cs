using QT.DependencyInjection;

namespace QT.Application.Entitys.Dto.FreshDelivery.CwCustomerInvoice;

/// <summary>
/// 开发票单输入参数.
/// </summary>
public class CwCustomerInvoiceListOutput
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
    /// 客户id.
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 开发票日期.
    /// </summary>
    public DateTime? receiptDate { get; set; }

    /// <summary>
    /// 开发票方式.
    /// </summary>
    public string paymentMethod { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 开发票金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 客户.
    /// </summary>
    public string cidName { get; set; }

    /// <summary>
    /// 公司id.
    /// </summary>
    public string oid { get; set; }
}

[SuppressSniffer]
public class CwCustomerInvoiceQueryOrderListOutput
{
    public string id { get; set; }
    public string no { get; set; }

    public string cid { get; set; }

    public string cidName { get; set; }

    public DateTime? createTime { get; set; }

    /// <summary>
    /// 应开发票
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 已开发票
    /// </summary>
    public decimal amount2 { get; set; }

    /// <summary>
    /// 未开发票
    /// </summary>
    public decimal amount3 { get; set; }

    /// <summary>
    /// 约定配送时间
    /// </summary>
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 入库数量
    /// </summary>
    public decimal inNum { get; set; }

    /// <summary>
    /// 特殊入库数量
    /// </summary>
    public decimal tsNum { get; set; }

    /// <summary>
    /// 采购单价
    /// </summary>
    public decimal? price { get; set; }
}