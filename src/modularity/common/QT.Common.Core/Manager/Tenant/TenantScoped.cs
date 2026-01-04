using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using QT.Common.Cache;
using QT.Common.Dtos.OAuth;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.Extras.DatabaseAccessor.SqlSugar;
using QT.FriendlyException;
using Serilog;
using SqlSugar;

namespace QT.Common.Core.Manager.Tenant;

/// <summary>
/// 租户范围作用域
/// </summary>
public class TenantScoped :/* ITenantScoped,*/ IScoped, ISqlSugarTenant
{
    private static AsyncLocal<string> _asyncLocalTenantId { get; set; } = new AsyncLocal<string>();


    private static AsyncLocal<ITenantDbInfo?> _asyncLocalTenant { get; set; } = new AsyncLocal<ITenantDbInfo?>();

    private static AsyncLocal<IServiceProvider> _asyncLocalServiceProvider = new AsyncLocal<IServiceProvider>();
    /// <inheritdoc/>
    public static string? TenantId
    {
        get
        {
            return _asyncLocalTenantId.Value;
            //if (_asyncLocalTenant.Value?.enCode != _asyncLocalTenantId.Value)
            //{

            //}

            //return _asyncLocalTenant.Value?.enCode; // ?? KeyVariable.DefaultDbConfigId;
        }
    }

    /// <summary>
    /// 存储服务，可能为空
    /// </summary>
    public static IServiceProvider? ServiceProvider => _asyncLocalServiceProvider.Value;

    /// <summary>
    /// 是否登录
    /// </summary>
    public bool IsLoggedIn => !string.IsNullOrEmpty(TenantId);



    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="tenantId"></param>
    //[ActivatorUtilitiesConstructor]
    //public TenantScoped(string tenantId)
    //{
    //    TenantId = tenantId;
    //}

    /// <inheritdoc/>
    //public void Login(string tenantId)
    //{
    //    this.TenantId = tenantId;
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    //public TenantScoped()
    //{

    //}

    /// <summary>
    /// 创建一个作用域范围
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="handler"></param>
    /// <param name="scopeFactory"></param>
    public static void Create(string tenantId, Action<IServiceScopeFactory, IServiceScope> handler, IServiceScopeFactory scopeFactory = default)
    {
        Scoped.Create((factory, scope) =>
        {
            LoginScoped(tenantId, scope.ServiceProvider);
            handler(factory, scope);

        }, scopeFactory);
    }
    /// <summary>
    /// 创建一个作用域范围
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="handler"></param>
    /// <param name="scopeFactory"></param>
    /// <returns></returns>
    public static async Task Create(string tenantId, Func<IServiceScopeFactory, IServiceScope, Task> handler, IServiceScopeFactory scopeFactory = default)
    {
        await Scoped.Create(async (factory, scope) =>
        {
            LoginScoped(tenantId, scope.ServiceProvider);
            await handler(factory, scope);
        }, scopeFactory);
    }

    /// <summary>
    /// 设置租户信息
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="serviceProvider"></param>
    public static void LoginScoped(string tenantId, IServiceProvider? serviceProvider)
    {
        _asyncLocalServiceProvider.Value = serviceProvider ?? App.RootServices;
        if (!string.IsNullOrEmpty(tenantId))
        {
            var _memoryCache = App.GetService<IMemoryCache>(serviceProvider);
            var list = _memoryCache.Get<List<ITenantDbInfo>>("tenant:dbcontext:list");

            // 判断租户是否存在
            if (list == null || !list.Any(x => x.enCode == tenantId))
            {
                var ex = Oops.Oh(ErrorCode.D1024);
                ex.StatusCode = 610001;
                throw ex;
            }

            _asyncLocalTenantId.Value = tenantId;
            _asyncLocalServiceProvider.Value = serviceProvider ?? App.RootServices;
            if (list.IsAny())
            {
                _asyncLocalTenant.Value = list.FirstOrDefault(x => x.enCode == tenantId);
            }



            var sqlSugarClient = App.GetService<ISqlSugarClient>(serviceProvider);
            if (sqlSugarClient is ITenant db)
            {
                db.ChangeDatabase(tenantId);
            }

            _ = App.GetService<ITenantManager>(serviceProvider);
            //Log.Information($"【{tenantId}】TenantScoped：{ts.GetHashCode()}，sqlSugarClient：{sqlSugarClient.GetHashCode()}，CurrentConnectionConfig：{sqlSugarClient.CurrentConnectionConfig.GetHashCode()}");
        }
    }

    /// <summary>
    /// 创建数据库链接
    /// </summary>
    /// <returns></returns>
    public ConnectionConfig? CreateConnection()
    {
        if (_asyncLocalTenant.Value is TenantInterFaceOutput tenant)
        {
            return SqlSugarHelper.CreateConnectionConfig(TenantId, tenant);
        }
        return null;
    }
}
