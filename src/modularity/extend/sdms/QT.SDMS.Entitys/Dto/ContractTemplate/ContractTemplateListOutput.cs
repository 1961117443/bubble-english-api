using QT.DependencyInjection;

namespace QT.SDMS.Entitys.Dto.ContractTemplate;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class ContractTemplateListOutput: ContractTemplateInfoOutput
{
    public DateTime? creatorTime { get; set; }
}
