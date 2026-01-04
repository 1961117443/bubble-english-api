using QT.DependencyInjection;
using SqlSugar;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrder;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class LogEnterpriseQuoteOrderCrInput
{
    /// <summary>
    /// 客户id.
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 订单日期
    /// </summary>
    public DateTime? billDate { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 发票类型.
    /// </summary>
    /// <returns></returns>
    public string? invoiceType { get; set; }

    /// <summary>
    /// 是否包含发票.
    /// </summary>
    /// <returns></returns>
    public int? invoice { get; set; }

    /// <summary>
    /// 商品明细
    /// </summary>
    public List<QuoteRecordInfoOutput> quoteRecordList { get; set; }

    /// <summary>
    /// 需方联系人员
    /// </summary>
    public string connectUser { get; set; }

    /// <summary>
    /// 需方联系电话
    /// </summary>
    public string connectPhone { get; set; }

    /// <summary>
    /// 需方联系地址
    /// </summary>
    public string connectAddress { get; set; }

    /// <summary>
    /// 供方联系人员
    /// </summary>
    public string supplyUser { get; set; }

    /// <summary>
    /// 供方联系电话
    /// </summary>
    public string supplyPhone { get; set; }

    /// <summary>
    /// 供方联系地址
    /// </summary>
    public string supplyAddress { get; set; }

    /// <summary>
    /// 模板id
    /// </summary>
    public string tid { get; set; }

    /// <summary>
    /// 模板参数 List<LogEnterpriseQuoteOrderTemplatePropertyInfo>
    /// </summary>
    public string templateJson { get; set; }
}
