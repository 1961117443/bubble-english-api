using QT.DependencyInjection;

namespace QT.Archive.Dto.Archives;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class ArchivesInfoOutput: ArchivesCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
