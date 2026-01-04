using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.WorkFlow.Entitys.Dto.FlowComment;

[SuppressSniffer]
public class FlowCommentListQuery : PageInputBase
{
    /// <summary>
    /// 任务id.
    /// </summary>
    public string? taskId { get; set; }
}

