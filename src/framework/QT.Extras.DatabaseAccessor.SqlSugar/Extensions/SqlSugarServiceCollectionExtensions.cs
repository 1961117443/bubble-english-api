using Microsoft.Extensions.Caching.Memory;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// SqlSugar 拓展类
/// </summary>
public static class SqlSugarServiceCollectionExtensions
{
    /// <summary>
    /// 添加 SqlSugar 拓展
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <param name="buildAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddSqlSugar(this IServiceCollection services, ConnectionConfig config, Action<ISqlSugarClient> buildAction = default)
    {
        var list = new List<ConnectionConfig>();
        list.Add(config);
        return services.AddSqlSugar(list, buildAction);
    }

    /// <summary>
    /// 添加 SqlSugar 拓展
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configs"></param>
    /// <param name="buildAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddSqlSugar(this IServiceCollection services, List<ConnectionConfig> configs, Action<ISqlSugarClient> buildAction = default)
    {
        // 注册 SqlSugar 客户端
        services.AddScoped<ISqlSugarClient>(u =>
        {
            List<ConnectionConfig> connectionConfigs = new List<ConnectionConfig>();
            //var tenant = u.GetRequiredService<ISqlSugarTenant>();
            //if (tenant.IsLoggedIn)
            //{
            //    //Console.WriteLine(tenant.CreateConnection());
            //    var conn = tenant.CreateConnection();
            //    if (conn!=null)
            //    {
            //        connectionConfigs.Add(conn);
            //    }
            //}
            //else
            {
                var cache = u.GetRequiredService<IMemoryCache>();
                var tenantList = cache.Get<List<ITenantDbInfo>>("tenant:dbcontext:list");
                var tenantConfigs = cache.Get<List<ConnectionConfig>>("tenant:dbcontext:configs");


                connectionConfigs.AddRange(configs);
                //List<ConnectionConfig> connectionConfigs = new List<ConnectionConfig>(configs);
                // 注册正常的租户数据库
                if (tenantList != null && tenantList.Count > 0 && tenantConfigs != null && tenantList.Count > 0)
                {
                    var list = tenantList.Where(it => it.enable).Select(it => it.enCode).ToArray();
                    tenantConfigs = tenantConfigs.Where(x => list.Contains(x.ConfigId.ToString()) && !configs.Any(it => it.ConfigId.ToString() == x.ConfigId.ToString())).ToList();

                    if (tenantConfigs.Count > 0)
                    {
                        connectionConfigs.AddRange(tenantConfigs);
                    }


                    // 如果存在多租户，去掉默认的数据库连接
                    var index = connectionConfigs.FindIndex(it => it.ConfigId.ToString() == "default");
                    if (index > -1)
                    {
                        connectionConfigs.RemoveAt(index);
                    }
                }
            }

            

            var sqlSugarClient = new SqlSugarClient(connectionConfigs);

            foreach (var config in connectionConfigs)
            {
                buildAction?.Invoke(sqlSugarClient.GetConnection(config.ConfigId));
            }
            //buildAction?.Invoke(sqlSugarClient);

            return sqlSugarClient;
        });

        // 注册 SqlSugar 仓储
        services.AddScoped(typeof(ISqlSugarRepository<>), typeof(SqlSugarRepository<>));

        services.AddScoped<SqlSugarUnitOfWorkFilter>();

        return services;
    }
}