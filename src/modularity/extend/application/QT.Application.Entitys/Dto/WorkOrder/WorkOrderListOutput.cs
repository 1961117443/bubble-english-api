using QT.Common;
using QT.DependencyInjection;
using QT.Extend.Entitys.Dto.ProjectGantt;

namespace QT.Extend.Entitys.Dto.WorkOrder;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class WorkOrderListOutput: WorkOrderInfoOutput
{
    /// <summary>
    /// 未读数量
    /// </summary>
    public int unread { get; set; }

    /// <summary>
    /// 最后回复人id
    /// </summary>
    public string lastReplayUserId { get; set; }

    /// <summary>
    /// 最后回复时间
    /// </summary>
    public DateTime? lastReplayTime { get; set; }

    /// <summary>
    /// 摘要
    /// </summary>
    public string zhaiyao => HtmlHelper.CutString(content, 100);
}
