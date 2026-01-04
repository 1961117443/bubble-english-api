using QT.DependencyInjection;

namespace QT.PRM.Dto.Visitor;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class VisitorListOutput: VisitorInfoOutput
{
    public string residentName { get; set; }
}
