using QT.DependencyInjection;

namespace QT.Archive.Dto.Document;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class DocumentUpInput : DocumentCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
