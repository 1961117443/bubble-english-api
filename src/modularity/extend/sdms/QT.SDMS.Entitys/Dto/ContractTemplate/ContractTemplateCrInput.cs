using QT.DependencyInjection;

namespace QT.SDMS.Entitys.Dto.ContractTemplate;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class ContractTemplateCrInput
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
    public List<ContractTemplatePropertyInfo> property { get; set; }
}

public class ContractTemplatePropertyInfo
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
