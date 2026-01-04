using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.WorkOrder;

/// <summary>
/// 
/// </summary>

[SuppressSniffer]
public class WorkOrderInfoOutput: WorkOrderCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }

    public string? no { get; set; }

    public DateTime? creatorTime { get; set; }

    public string? creatorUserId { get; set; }

    public string? creatorUserIdName { get; set; }

    /// <summary>
    /// 是否关闭
    /// </summary>
    public int enabledMark { get; set; }


    /// <summary>
    /// 处理人id
    /// </summary>
    public string? assignUserId { get; set; }

    /// <summary>
    /// 处理人姓名
    /// </summary>
    public string? assignUserIdName { get; set; }

    /// <summary>
    /// 团队名称
    /// </summary>
    public string tidName { get; set; }

    /// <summary>
    /// 项目名称
    /// </summary>
    public string pidName { get; set; }

    /// <summary>
    /// 状态(status =0 草稿，status=1 已提交)
    /// </summary>
    public int? status { get; set; }
}
