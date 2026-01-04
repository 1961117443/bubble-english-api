namespace QT.TaskScheduler;

/// <summary>
/// 定时器执行状态器
/// </summary>
[SuppressSniffer]
public sealed class SpareTimerExecuter
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="timer"></param>
    /// <param name="status"></param>
    public SpareTimerExecuter(SpareTimer timer, int status)
    {
        Timer = timer;
        Status = status;
    }

    /// <summary>
    /// 定时器
    /// </summary>
    public SpareTimer Timer { get; internal set; }

    /// <summary>
    /// 状态
    /// </summary>
    /// <remarks>
    /// <para>0：任务开始</para>
    /// <para>1：执行之前</para>
    /// <para>2：执行成功</para>
    /// <para>3：执行失败</para>
    /// <para>-1：任务停止</para>
    /// <para>-2：任务取消</para>
    /// <para>99：检查执行节点</para>
    /// </remarks>
    public int Status { get; internal set; }
}