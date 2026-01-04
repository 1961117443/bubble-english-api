using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.TaskScheduler.Entitys.Dto.TaskScheduler;

[SuppressSniffer]
public class TaskLogInput : PageInputBase
{
    /// <summary>
    /// 执行结果.
    /// </summary>
    public int? runResult { get; set; }

    /// <summary>
    /// 开始时间.
    /// </summary>
    public long? startTime { get; set; }

    /// <summary>
    /// 结束时间.
    /// </summary>
    public long? endTime { get; set; }
}
