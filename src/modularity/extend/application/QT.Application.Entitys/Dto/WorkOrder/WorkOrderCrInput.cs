using QT.Application.Entitys.Enum;
using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.WorkOrder;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class WorkOrderCrInput
{
    /// <summary>
    /// 工单类型.
    /// </summary>
    public WorkOrderCategory category { get; set; }

    /// <summary>
    /// 处理人
    /// </summary>
    public string? assignUserId { get; set; }

    /// <summary>
    /// 团队id.
    /// </summary>
    public string? tid { get; set; }

    /// <summary>
    /// 项目id.
    /// </summary>
    public string? pid { get; set; }


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

    /// <summary>
    /// 状态(status =0 草稿，status=1 已提交)
    /// </summary>
    public int? status { get; set; }
}
