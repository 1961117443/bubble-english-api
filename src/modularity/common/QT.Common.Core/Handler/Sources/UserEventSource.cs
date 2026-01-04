using QT.EventBus;
using QT.Systems.Entitys.Permission;

namespace QT.EventHandler;

/// <summary>
/// 用户事件源.
/// </summary>
public class UserEventSource : IEventSource
{
    /// <summary>
    /// 构造函数.
    /// </summary>
    /// <param name="eventId">事件ID.</param>
    /// <param name="tenantId">租户ID.</param>
    /// <param name="enantDbName">租户数据库名称.</param>
    /// <param name="entity">实体.</param>
    public UserEventSource(string eventId, string tenantId, string enantDbName, UserEntity entity)
    {
        EventId = eventId;
        TenantId = tenantId;
        TenantDbName = enantDbName;
        Entity = entity;
    }

    /// <summary>
    /// 租户ID.
    /// </summary>
    public string TenantId { get; set; }

    /// <summary>
    /// 租户数据库名称.
    /// </summary>
    public string TenantDbName { get; set; }

    /// <summary>
    /// 用户实体.
    /// </summary>
    public UserEntity Entity { get; set; }

    /// <summary>
    /// 事件 Id.
    /// </summary>
    public string EventId { get; }

    /// <summary>
    /// 事件承载（携带）数据.
    /// </summary>
    public object Payload { get; }

    /// <summary>
    /// 取消任务 Token.
    /// </summary>
    /// <remarks>用于取消本次消息处理.</remarks>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// 事件创建时间.
    /// </summary>
    public DateTime CreatedTime { get; } = DateTime.UtcNow;
}