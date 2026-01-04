namespace QT.EventBus;

/// <summary>
/// 事件处理程序执行后上下文
/// </summary>
public sealed class EventHandlerExecutedContext : EventHandlerContext
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="eventSource">事件源（事件承载对象）</param>
    /// <param name="properties">共享上下文数据</param>
    internal EventHandlerExecutedContext(IEventSource eventSource, IDictionary<object, object> properties)
        : base(eventSource, properties)
    {
    }

    /// <summary>
    /// 执行后时间
    /// </summary>
    public DateTime ExecutedTime { get; internal set; }

    /// <summary>
    /// 异常信息
    /// </summary>
    public InvalidOperationException Exception { get; internal set; }
}