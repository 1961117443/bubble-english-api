using QT.Common.Configuration;
using QT.Common.Options;
using QT.Systems.Entitys.Permission;
using QT.EventBus;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using Microsoft.Extensions.Options;
using QT.Common.Core.Manager.Tenant;

namespace QT.EventHandler;

/// <summary>
/// 用户事件订阅.
/// </summary>
public class UserEventSubscriber : IEventSubscriber
{
    public IServiceProvider Services { get; }

    private readonly ConnectionStringsOptions _connectionStrings;

    /// <summary>
    /// 构造函数.
    /// </summary>
    public UserEventSubscriber(
        IServiceProvider services,
        IOptions<ConnectionStringsOptions> connectionOptions)
    {
        Services = services;
        _connectionStrings = connectionOptions.Value;
    }

    /// <summary>
    /// 修改用户登录信息.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe("User:UpdateUserLogin")]
    public async Task UpdateUserLoginInfo(EventHandlerExecutingContext context)
    {
        var log = (UserEventSource)context.Source;
        using var scope = Services.CreateScope();
        var _repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<UserEntity>>();
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

        await _repository.Context.Updateable(log.Entity).UpdateColumns(m => new { m.FirstLogIP, m.FirstLogTime, m.PrevLogTime, m.PrevLogIP, m.LastLogTime, m.LastLogIP, m.LogSuccessCount }).ExecuteCommandAsync();
    }
}