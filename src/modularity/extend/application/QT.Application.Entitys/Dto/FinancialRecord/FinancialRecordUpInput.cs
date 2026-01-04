using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.FinancialRecord;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class FinancialRecordUpInput : FinancialRecordCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}
