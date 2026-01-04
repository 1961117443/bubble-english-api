using QT.DependencyInjection;

namespace QT.Archive.Dto.Rule;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class RuleUpInput : RuleCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
