using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrderShare;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class LogEnterpriseQuoteOrderShareInfoOutput : LogEnterpriseQuoteOrderShareCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }

    /// <summary>
    /// 网页地址
    /// </summary>
    public string webUrl { get; set; }

    /// <summary>
    /// 微信小程序地址
    /// </summary>
    public string miniUrl { get; set; }
}
