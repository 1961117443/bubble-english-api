using QT.Systems.Entitys.System;
using QT.EventBus;

namespace QT.EventHandler
{
    /// <summary>
    /// 日记事件源（事件承载对象）.
    /// </summary>
    public class TaskLogEventSource : IEventSource
    {
        /// <summary>
        /// 构造函数.
        /// </summary>
        /// <param name="eventId">事件ID.</param>
        /// <param name="tenantId">租户ID.</param>
        /// <param name="enantDbName">租户数据库名称.</param>
        /// <param name="entity">实体.</param>
        public TaskLogEventSource(string eventId, string tenantId, string enantDbName, TimeTaskLogEntity entity)
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
        /// 日记实体.
        /// </summary>
        public TimeTaskLogEntity Entity { get; set; }

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
}