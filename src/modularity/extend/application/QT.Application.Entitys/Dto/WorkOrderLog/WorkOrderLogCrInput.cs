using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.WorkOrderLog;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class WorkOrderLogCrInput
{
    /// <summary>
    /// 工单id.
    /// </summary>
    public string? wid { get; set; }


    /// <summary>
    /// 问题描述.
    /// </summary>
    public string? content { get; set; }

    /// <summary>
    /// 图片.
    /// </summary>
    public string? imageJson { get; set; }


    /// <summary>
    /// 附件.
    /// </summary>
    public string? attachJson { get; set; }
}
