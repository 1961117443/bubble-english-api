using QT.Common.Configuration;
using QT.Common.Options;
using QT.EventBus;
using QT.Systems.Entitys.System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlSugar;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Core.Entity;
using Mapster;

namespace QT.EventHandler;

/// <summary>
/// 日记事件订阅.
/// </summary>
public class LogEventSubscriber : IEventSubscriber
{
    public IServiceProvider Services { get; }

    private readonly ConnectionStringsOptions _connectionStrings;

    /// <summary>
    /// 构造函数.
    /// </summary>
    public LogEventSubscriber(
        IServiceProvider services,
        IOptions<ConnectionStringsOptions> connectionOptions)
    {
        Services = services;
        _connectionStrings = connectionOptions.Value;
    }

    /// <summary>
    /// 创建日记.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe("Log:CreateReLog")]
    [EventSubscribe("Log:CreateExLog")]
    [EventSubscribe("Log:CreateVisLog")]
    [EventSubscribe("Log:CreateOpLog")]
    public async Task CreateLog(EventHandlerExecutingContext context)
    {
        var log = (LogEventSource)context.Source;
        using var scope = Services.CreateScope();
        var _repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<SysLogEntity>>();
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

        await _repository.Context.UseLogDatabase().Insertable(log.Entity).IgnoreColumns(ignoreNullColumn: true).SplitTable().ExecuteCommandAsync();

        if (log.Entity.Category == 1)
        {
            // 登录日志单独记录
            await _repository.Context.Insertable<SysLoginLog>(log.Entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 创建任务日记.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe("Log:CreateTaskLog")]
    public async Task CreateTaskLog(EventHandlerExecutingContext context)
    {
        var log = (TaskLogEventSource)context.Source;
        using var scope = Services.CreateScope();
        var _repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<TimeTaskLogEntity>>();
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

        await _repository.Context.UseLogDatabase().Insertable(log.Entity).IgnoreColumns(ignoreNullColumn: true).SplitTable().ExecuteCommandAsync();
    }


    /// <summary>
    /// 创建差异日记.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe("Log:CreateDiffLog")]
    public async Task CreateDiffLog(EventHandlerExecutingContext context)
    {
        var log = context.Source.Payload.Adapt<TenantLogEventSource>();
        if (log != null)
        {
            using var scope = Services.CreateScope();
            var _repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<SysLogDiff>>();
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
            }
            var logEntity = log.Entity.Adapt<SysLogDiff>();
            await _repository.Context.UseLogDatabase().Insertable<SysLogDiff>(logEntity).IgnoreColumns(ignoreNullColumn: true).SplitTable().ExecuteCommandAsync();
        }
    }
}