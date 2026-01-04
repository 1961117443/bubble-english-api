using QT.Common.Security;
using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Archive.Dto.Document;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class DocumentListOutput: DocumentInfoOutput
{
    public DateTime? creatorTime { get; set; }
}