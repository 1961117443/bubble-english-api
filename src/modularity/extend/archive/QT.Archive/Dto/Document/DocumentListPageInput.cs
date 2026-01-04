using QT.Common.Filter;
using System.ComponentModel.DataAnnotations;

namespace QT.Archive.Dto.Document;

public class DocumentListPageInput: PageInputBase
{
    public string aid { get; set; }
}