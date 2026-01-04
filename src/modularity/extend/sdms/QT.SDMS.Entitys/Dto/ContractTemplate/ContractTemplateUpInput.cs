using QT.DependencyInjection;

namespace QT.SDMS.Entitys.Dto.ContractTemplate;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class ContractTemplateUpInput : ContractTemplateCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
