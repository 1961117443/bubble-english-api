using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using QT;
using QT.Common.Cache;
using QT.Common.Configuration;
using QT.Common.Dtos.OAuth;
using QT.Common.Extension;
using QT.Common.Options;
using QT.FriendlyException;
using SqlSugar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSugar;

public static class SqlSugarHelper
{
    /// <summary>
    /// 创建数据库连接配置
    /// </summary>
    /// <param name="tenantInterFace">租户的配置信息</param>
    /// <returns></returns>
    public static ConnectionConfig CreateConnectionConfig(string tenantId, TenantInterFaceOutput tenant)
    {
        //var t = _caches.GetOrAdd(tenantId, _ => typeof(SqlSugarCache<>).MakeGenericType(TenantCacheFactory.GetOrCreateMark(tenantId)));
        var t = TenantCacheFactory.GetOrCreate(CacheType.SqlSugarCache, tenantId);
        var cacheService = App.GetService(t) as ICacheService ?? new SqlSugarCache();
        if (tenant.linkList.IsAny())
        {
            // 先找出主数据库配置
            var master = tenant.linkList.Find(it => it.configType == 0) ?? tenant.linkList.First();

            var connectionConfig = new ConnectionConfig()
            {
                DbType = (DbType)Enum.Parse(typeof(DbType), master.dbType, ignoreCase: true),
                ConfigId = tenantId, // 设置库的唯一标识
                IsAutoCloseConnection = true,
                ConnectionString = GenerateConnectionString(master),
                ConfigureExternalServices = new ConfigureExternalServices
                {
                    DataInfoCacheService = cacheService // new SqlSugarCache()// Activator.CreateInstance(t) as ICacheService // new SqlSugarCache()
                     ,
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
                },
                MoreSettings = new ConnMoreSettings
                {
                    IsAutoRemoveDataCache = true
                }
            };


            var slaves = tenant.linkList.Except(new[] { master });
            //配置从库
            if (slaves.IsAny())
            {
                connectionConfig.SlaveConnectionConfigs = slaves.Select(it => new SlaveConnectionConfig
                {
                    ConnectionString = GenerateConnectionString(it)
                }).ToList();
            }

            return connectionConfig;
        }
        else
        {
            var options = App.GetOptions<ConnectionStringsOptions>();
            var connectionConfig = new ConnectionConfig()
            {
                DbType = (DbType)Enum.Parse(typeof(DbType), options.DBType),
                ConfigId = tenantId, // 设置库的唯一标识
                IsAutoCloseConnection = true,
                ConnectionString = string.Format(options.DefaultConnection, tenant.dotnet),
                ConfigureExternalServices = new ConfigureExternalServices
                {
                    DataInfoCacheService = cacheService //new SqlSugarCache()// Activator.CreateInstance(t) as ICacheService // new SqlSugarCache()
                    ,
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
                },
                MoreSettings = new ConnMoreSettings
                {
                    IsAutoRemoveDataCache = true
                }
            };

            return connectionConfig;
        }
    }



    #region 私有方法

    /// <summary>
    /// 创建数据库连接
    /// </summary>
    /// <param name="link"></param>
    /// <returns></returns>
    private static string GenerateConnectionString(TenantLinkModel link)
    {
        if (!string.IsNullOrEmpty(link.connectionStr))
        {
            return link.connectionStr;
        }
        var DbType = (DbType)Enum.Parse(typeof(DbType), link.dbType, ignoreCase: true);

        switch (DbType)
        {
            case SqlSugar.DbType.MySql:
                return $"server={link.host};Port={link.port};Database={link.serviceName};Uid={link.userName};Pwd={link.password};AllowLoadLocalInfile=true;SslMode=none;AllowPublicKeyRetrieval=True;";
            case SqlSugar.DbType.SqlServer:
                return string.Format("Data Source={0},{4};Initial Catalog={1};User ID={2};Password={3};MultipleActiveResultSets=true", link.host, link.serviceName, link.userName, link.password, link.port);
            case SqlSugar.DbType.Sqlite:
                break;
            case SqlSugar.DbType.Oracle:
                return string.Format("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVER = DEDICATED)(SERVICE_NAME={2})));User Id={3};Password={4}", link.host, link.port.ToString(), link.dbSchema, link.userName, link.password);
            case SqlSugar.DbType.PostgreSQL:
                break;
            case SqlSugar.DbType.Dm:
                break;
            case SqlSugar.DbType.Kdbndp:
                break;
            case SqlSugar.DbType.Oscar:
                break;
            case SqlSugar.DbType.MySqlConnector:
                break;
            case SqlSugar.DbType.Access:
                break;
            case SqlSugar.DbType.OpenGauss:
                break;
            case SqlSugar.DbType.QuestDB:
                break;
            case SqlSugar.DbType.HG:
                break;
            case SqlSugar.DbType.ClickHouse:
                break;
            case SqlSugar.DbType.GBase:
                break;
            case SqlSugar.DbType.Odbc:
                break;
            case SqlSugar.DbType.OceanBaseForOracle:
                break;
            case SqlSugar.DbType.TDengine:
                break;
            case SqlSugar.DbType.GaussDB:
                break;
            case SqlSugar.DbType.OceanBase:
                break;
            case SqlSugar.DbType.Tidb:
                break;
            case SqlSugar.DbType.Vastbase:
                break;
            case SqlSugar.DbType.Custom:
                break;
            default:
                break;
        }

        throw Oops.Oh($"[{DbType}]数据库类型，没有找到合适的链接模板！");
    }
    #endregion
}

public static class QTSqlFunc
{
    /// <summary>
    /// 扩展mysql  FIND_IN_SET
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="str"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static bool FIND_IN_SET(string str,string value)
    {
        //这里不能写任何实现代码，需要在上面的配置中实现
        throw new NotSupportedException("Can only be used in expressions");
    }
}