using QT.DependencyInjection;
using QT.TaskScheduler.Entitys.Entity;

namespace QT.TaskScheduler.Entitys.Dto.TaskScheduler;

[SuppressSniffer]
public class TimeTaskUpInput:TimeTaskCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}

[SuppressSniffer]
public class TimeTaskSyncModel
{
    /// <summary>
    /// 租户
    /// </summary>
    public string TenantId { get; set; }

    /// <summary>
    /// 发布所在的机器码
    /// </summary>
    public string WorkerId { get; set; }

    /// <summary>
    /// 业务数据
    /// </summary>
    public TimeTaskEntity Data { get; set; }
}