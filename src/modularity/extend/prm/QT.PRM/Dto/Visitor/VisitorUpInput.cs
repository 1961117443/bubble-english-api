using QT.DependencyInjection;

namespace QT.PRM.Dto.Visitor;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class VisitorUpInput : VisitorCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
