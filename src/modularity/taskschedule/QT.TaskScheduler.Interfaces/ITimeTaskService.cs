using QT.TaskScheduler.Entitys.Entity;

namespace QT.TaskScheduler.Interfaces.TaskScheduler;

/// <summary>
/// 定时任务
/// </summary>
public interface ITimeTaskService
{
    /// <summary>
    /// 添加定时任务
    /// </summary>
    /// <param name="input"></param>
    void AddTimerJob(TimeTaskEntity input);

    /// <summary>
    /// 启动自启动任务.
    /// </summary>
    void StartTimerJob();
}
