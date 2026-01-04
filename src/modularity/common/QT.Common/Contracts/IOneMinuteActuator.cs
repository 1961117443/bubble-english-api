namespace QT.Common.Contracts;

/// <summary>
/// 1分钟定时任务
/// </summary>
public interface IOneMinuteActuator
{
    /// <summary>
    /// 执行任务
    /// </summary>
    /// <returns></returns>
    Task Execute();
}