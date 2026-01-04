using QT.DependencyInjection;

namespace QT.Common.Models.NPOI;

/// <summary>
/// Excel导出模板
/// </summary>
[SuppressSniffer]
public class ExcelTemplateModel
{
    /// <summary>
    /// 行号.
    /// </summary>
    public int row { get; set; }

    /// <summary>
    /// 列号.
    /// </summary>
    public int cell { get; set; }

    /// <summary>
    /// 数据值.
    /// </summary>
    public string? value { get; set; }
}