using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.QuoteOrderTemplate;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class QuoteOrderTemplateCrInput
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
    public List<QuoteOrderTemplatePropertyInfo> property { get; set; }
}

public class QuoteOrderTemplatePropertyInfo
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
