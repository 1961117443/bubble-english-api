using QT.DependencyInjection;

namespace QT.Apps.Entitys.Dto;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class AppMenuListOutput : AppDataListAllOutput
{
    /// <summary>
    /// 排序码.
    /// </summary>
    public long? sortCode { get; set; }
}