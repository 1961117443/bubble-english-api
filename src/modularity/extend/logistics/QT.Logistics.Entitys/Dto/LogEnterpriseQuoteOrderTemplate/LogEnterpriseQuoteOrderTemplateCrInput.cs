using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrderTemplate;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class LogEnterpriseQuoteOrderTemplateCrInput
{
    /// <summary>
    /// 模板名称.
    /// </summary>
    public string? name { get; set; }

    /// <summary>
    /// 模板文件.
    /// </summary>
    public string? fileUrl { get; set; }


    /// <summary>
    /// 模板参数.
    /// </summary>
    public List<LogEnterpriseQuoteOrderTemplatePropertyInfo> property { get; set; }
}

public class LogEnterpriseQuoteOrderTemplatePropertyInfo
{
    /// <summary>
    /// 参数
    /// </summary>
   public string? name { get; set; }

    /// <summary>
    /// 参数值
    /// </summary>
    public string value { get; set; }

}
