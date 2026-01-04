using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.Label;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class LabelInfoOutput: LabelCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
