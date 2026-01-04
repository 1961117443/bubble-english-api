using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrder;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class LogEnterpriseQuoteOrderInfoOutput: LogEnterpriseQuoteOrderCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }

    /// <summary>
    /// 发票类型描述.
    /// </summary>
    /// <returns></returns>
    public string? invoiceTypeCn { get; set; }

    /// <summary>
    /// 创建人id
    /// </summary>
    public string? creatorUserId { get; set; }
    /// <summary>
    /// 报价人.
    /// </summary>
    /// <returns></returns>
    public string? quoterCn { get; set; }
    public string? eid { get; set; }
}

public class QuoteRecordInfoOutput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }

    public string name{ get; set; }

    public string spec { get; set;}

    public string unit { get; set;}

    public string remark { get; set;}

    public decimal? num { get; set; }

    public decimal? price { get; set; }

    public decimal? amount { get; set; }

}