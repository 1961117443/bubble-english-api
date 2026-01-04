using QT.Common.Configuration;
using QT.Common.Options;
using QT.EventBus;
using QT.Systems.Entitys.System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlSugar;
using QT.Common.Const;
using QT.Common.Core.Manager.Tenant;

namespace QT.EventHandler;

/// <summary>
/// 任务事件订阅.
/// </summary>
public class TaskEventSubscriber : IEventSubscriber
{
    public IServiceProvider Services { get; }

    private readonly ConnectionStringsOptions _connectionStrings;

    /// <summary>
    /// 构造函数.
    /// </summary>
    public TaskEventSubscriber(
        IServiceProvider services,
        IOptions<ConnectionStringsOptions> connectionOptions)
    {
        Services = services;
        _connectionStrings = connectionOptions.Value;
    }

    /// <summary>
    /// 创建任务日记.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe("Task:UpdateTask")]
    public async Task UpdateTask(EventHandlerExecutingContext context)
    {
        var log = (TaskEventSource)context.Source;
        using var scope = Services.CreateScope();
        var _repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<TimeTaskEntity>>();
        if (KeyVariable.MultiTenancy)
        {
            if (log.TenantId == null) return;
            if (TenantManager.Login(log.TenantId, _repository.Context))
            {
                _repository.Context.ChangeDatabase(log.TenantId);
            }
            else
            {
                return;
            }
            //_repository.Context.AddConnection(new ConnectionConfig()
            //{
            //    DbType = (DbType)Enum.Parse(typeof(DbType), _connectionStrings.DBType),
            //    ConfigId = log.TenantId, // 设置库的唯一标识
            //    IsAutoCloseConnection = true,
            //    ConnectionString = string.Format(_connectionStrings.DefaultConnection, log.TenantDbName)
            //});
            //_repository.Context.ChangeDatabase(log.TenantId);
        }

        await _repository.Context.Updateable<TimeTaskEntity>().SetColumns(x => new TimeTaskEntity()
        {
            RunCount = x.RunCount + 1,
            LastRunTime = DateTime.Now,
            NextRunTime = log.Entity.NextRunTime,
            LastModifyUserId = CommonConst.SUPPER_ADMIN_ACCOUNT,
            LastModifyTime = DateTime.Now
        }).Where(x => x.Id == log.Entity.Id).ExecuteCommandAsync();
    }
}