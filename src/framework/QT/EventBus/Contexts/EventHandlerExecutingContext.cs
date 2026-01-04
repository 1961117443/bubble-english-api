namespace QT.EventBus;

/// <summary>
/// 事件处理程序执行前上下文
/// </summary>
public sealed class EventHandlerExecutingContext : EventHandlerContext
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="eventSource">事件源（事件承载对象）</param>
    /// <param name="properties">共享上下文数据</param>
    internal EventHandlerExecutingContext(IEventSource eventSource, IDictionary<object, object> properties)
        : base(eventSource, properties)
    {
    }

    /// <summary>
    /// 执行前时间
    /// </summary>
    public DateTime ExecutingTime { get; internal set; }
}