using Polly;
using QT;
using QT.Common.Cache;
using QT.Common.Configuration;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Extension;
using QT.Common.Options;
using QT.Extras.DatabaseAccessor.SqlSugar;
using QT.FriendlyException;
using QT.Systems.Entitys.Permission;
using SqlSugar;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// SqlSugar配置拓展.
/// </summary>
public static class SqlSugarConfigureExtensions
{
    public static IServiceCollection SqlSugarConfigure(this IServiceCollection services)
    {
        // 获取选项
        ConnectionStringsOptions connectionStrings = App.GetConfig<ConnectionStringsOptions>("ConnectionStrings", true);

        List<ConnectionConfig> connectConfigList = new List<ConnectionConfig>();

        string? connectionStr = connectionStrings.DefaultConnection;
        var dataBase = connectionStrings.DBName;
        var DBType = (DbType)Enum.Parse(typeof(DbType), connectionStrings.DBType);
        var ConfigId = connectionStrings.ConfigId;
        var DBName = connectionStrings.DBName;

        // 默认数据库
        connectConfigList.Add(new ConnectionConfig
        {
            ConnectionString = string.Format(connectionStr, DBName),
            DbType = DBType,
            IsAutoCloseConnection = true,
            ConfigId = ConfigId,
            InitKeyType = InitKeyType.Attribute,
            ConfigureExternalServices = new ConfigureExternalServices
            {
                DataInfoCacheService = new SqlSugarCache(),
                SqlFuncServices = new List<SqlFuncExternal>()
                    {
                        new SqlFuncExternal
                        {
                            UniqueMethodName = nameof(QTSqlFunc.FIND_IN_SET),
                            MethodValue = (expInfo, dbType, expContext) =>
                            {
                                if (dbType == DbType.MySql)
                                {
                                    return $" FIND_IN_SET('{expInfo.Args[0].MemberValue}',{expInfo.Args[1].MemberName})";
                                }
                                else
                                {
                                    throw new Exception("未实现");
                                }
                            }
                        }
                    }
            }
        });

        services.AddSqlSugar(connectConfigList, db => db.InitDb());

        ////获取所有分表名的方法
        //StaticConfig.SplitTableGetTablesFunc = () =>
        //{
        //    //优化技巧：每天12点准时清空这个缓存,因为分表都是12点之后才创建新的
        //    var cache = App.GetService<ICacheManager>();
        //    //TenantScoped
        //    return cache.GetOrCreate("SplitTableInfo", entry =>
        //    {
        //        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2);
        //        var db = App.GetService<ISqlSugarClient>();
        //        var list = db.DbMaintenance.GetTableInfoList(false);
        //        var result = list.Select(it => new SplitTableInfo() { TableName = it.Name }).ToList();
        //        return result;
        //    });
        //};

        /*
        services.AddSqlSugar(connectConfigList, db =>
        {
            var sqlSugarInsertByObjects = App.RootServices?.GetServices<ISqlSugarInsertByObject>();
            var sqlSugarUpdateByObjects = App.RootServices?.GetServices<ISqlSugarUpdateByObject>();
            var sqlSugarDeleteByObjects = App.RootServices?.GetServices<ISqlSugarDeleteByObject>();
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
                //Console.WriteLine("Default:{0}", db.CurrentConnectionConfig.ConfigId);
                if (KeyVariable.MultiTenancy)
                {
                    if (db.CurrentConnectionConfig.ConfigId.ToString() == KeyVariable.DefaultDbConfigId)
                    {
                        throw Oops.Oh("默认数据库禁止使用！");
                    }
                }
#if DEBUG
                if (sql.StartsWith("SELECT"))
                    Console.ForegroundColor = ConsoleColor.Green;

                if (sql.StartsWith("UPDATE") || sql.StartsWith("INSERT"))
                    Console.ForegroundColor = ConsoleColor.White;

                if (sql.StartsWith("DELETE"))
                    Console.ForegroundColor = ConsoleColor.Blue;

                // 在控制台输出sql语句
                Console.WriteLine(SqlProfiler.ParameterFormat(sql, pars));
                Console.WriteLine();

                // 在MiniProfiler内显示
                // App.PrintToMiniProfiler("SqlSugar", "Info", SqlProfiler.ParameterFormat(sql, pars));
#endif
            };

            db.Aop.DataExecuting = (oldValue, entityInfo) =>
            {
                if (entityInfo.OperationType == DataFilterType.InsertByObject)
                {
                    if (entityInfo.PropertyName == "CreatorTime" )
                        entityInfo.SetValue(DateTime.Now);
                    if (App.User != null)
                    {
                        //if (entityInfo.PropertyName == nameof(ICompanyEntity.Oid) && entityInfo.EntityValue is ICompanyEntity companyEntity && string.IsNullOrEmpty(companyEntity.Oid))
                        //{
                        //    companyEntity.Oid = GetCompanyId();
                        //    //entityInfo.SetValue(GetCompanyId());
                        //}
                        if (entityInfo.PropertyName == "CreatorUserId")
                        {
                            var creatorUserId = ((dynamic)entityInfo.EntityValue).CreatorUserId;
                            if (string.IsNullOrEmpty(creatorUserId))
                                entityInfo.SetValue(App.User.FindFirst(ClaimConst.CLAINMUSERID)?.Value);
                        }
                    }

                    if (sqlSugarInsertByObjects != null && sqlSugarInsertByObjects.IsAny())
                    {
                        foreach (var item in sqlSugarInsertByObjects)
                        {
                            item.Execute(entityInfo);
                        }
                    }
                }
                if (entityInfo.OperationType == DataFilterType.UpdateByObject)
                {
                    if (entityInfo.PropertyName == "LastModifyTime")
                        entityInfo.SetValue(DateTime.Now);
                    if (entityInfo.PropertyName == "LastModifyUserId")
                        entityInfo.SetValue(App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value);

                    if (sqlSugarUpdateByObjects != null && sqlSugarUpdateByObjects.IsAny())
                    {
                        foreach (var item in sqlSugarUpdateByObjects)
                        {
                            item.Execute(entityInfo);
                        }
                    }
                }
                if (entityInfo.OperationType == DataFilterType.DeleteByObject)
                {
                    if (sqlSugarDeleteByObjects!=null && sqlSugarDeleteByObjects.IsAny())
                    {
                        foreach (var item in sqlSugarDeleteByObjects)
                        {
                            item.Execute(entityInfo);
                        }
                    }
                }
            };

            var services = App.RootServices?.GetServices<ISqlSugarQueryFilter>();
            if (services!=null && services.IsAny())
            {
                foreach (var item in services)
                {
                    item.Execute(db);
                }
            }

            //// 全局添加公司过滤
            //db.QueryFilter.AddTableFilterIF<ICompanyEntity>(CompanyFilterCheck(), x => x.Oid == GetCompanyId());
        });
        */

        // 设置默认的缓存
        SqlSugar.DefaultServices.DataInoCache = new SqlSugarCache();

        // 添加全局过滤器
        services.AddMvcFilter<SqlSugarUnitOfWorkFilter>();
        return services;
    }

    ///// <summary>
    ///// 判断是否需要添加公司过滤
    ///// </summary>
    ///// <returns></returns>
    //private static bool CompanyFilterCheck()
    //{
    //    if (App.User == null) return false;
    //    if (App.HttpContext == null) return false;
    //    //Console.WriteLine("CompanyFilterCheck...................1");
    //    // 集团账号不过滤
    //    if (!App.HttpContext.Items.TryGetValue(ClaimConst.CLAINM_JT_COMPANY_ACCOUNT, out var companyAccount))
    //    {
    //        //Console.WriteLine("CompanyFilterCheck...................2");
    //        string cacheKey = string.Format("{0}{1}_{2}", CommonConst.CACHEKEYONLINEUSER, App.User.FindFirst(ClaimConst.CLAINMUSERID)?.Value, "company");
    //        var org = App.GetService<ICacheManager>().Get<OrganizeEntity>(cacheKey);
    //        if (org != null)
    //        {
    //            companyAccount = org.EnCode != "JT";
    //            App.HttpContext.Items.TryAdd(ClaimConst.CLAINM_JT_COMPANY_ACCOUNT, companyAccount);
    //            App.HttpContext.Items.TryAdd(ClaimConst.CLAINMCOMPANYID, org.Id);
    //        }
    //    };
    //    return !string.IsNullOrEmpty(companyAccount?.ToString()) && bool.TryParse(companyAccount?.ToString(), out bool result) && result;
    //    //return App.User.FindFirst(ClaimConst.CLAINM_JT_COMPANY_ACCOUNT)?.Value != "1";
    //}

    ///// <summary>
    ///// 获取公司id
    ///// </summary>
    ///// <returns></returns>
    //private static string? GetCompanyId()
    //{
    //    if (!App.HttpContext.Items.TryGetValue(ClaimConst.CLAINMCOMPANYID, out var companyId))
    //    {
    //        string cacheKey = string.Format("{0}{1}_{2}", CommonConst.CACHEKEYONLINEUSER, App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value, "company");
    //        var org = App.GetService<ICacheManager>().Get<OrganizeEntity>(cacheKey);
    //        if (org != null)
    //        {
    //            companyId = org.Id;
    //            App.HttpContext.Items.TryAdd(ClaimConst.CLAINMCOMPANYID, org.Id);
    //        }
    //    }
    //    return companyId?.ToString() ?? string.Empty;
    //}
}