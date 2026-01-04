using QT.DependencyInjection;

namespace QT.Archive.Dto.Document;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class DocumentInfoOutput: DocumentCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
