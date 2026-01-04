using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.Schedule;

/// <summary>
/// 更新日程安排.
/// </summary>
[SuppressSniffer]
public class ScheduleUpInput : ScheduleCrInput
{
    /// <summary>
    /// 主键id.
    /// </summary>
    public string? id { get; set; }
}
