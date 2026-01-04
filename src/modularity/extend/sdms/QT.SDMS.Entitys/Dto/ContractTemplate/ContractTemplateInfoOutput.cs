using QT.DependencyInjection;

namespace QT.SDMS.Entitys.Dto.ContractTemplate;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class ContractTemplateInfoOutput: ContractTemplateCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
