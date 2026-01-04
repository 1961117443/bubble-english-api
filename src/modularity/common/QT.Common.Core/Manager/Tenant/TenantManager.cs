using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using QT.Common.Cache;
using QT.Common.Configuration;
using QT.Common.Const;
using QT.Common.Contracts;
using QT.Common.Core.Entity;
using QT.Common.Core.Security;
using QT.Common.Dtos.OAuth;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.EventBus;
using QT.EventHandler;
using QT.Extras.DatabaseAccessor.SqlSugar;
using QT.FriendlyException;
using QT.JsonSerialization;
using QT.RemoteRequest.Extensions;
using QT.UnifyResult;
using SqlSugar;
using System.Diagnostics.CodeAnalysis;

namespace QT.Common.Core.Manager.Tenant;

/// <summary>
/// 租户管理
/// </summary>
public class TenantManager : ITenantManager, IScoped/*, ISqlSugarTenant*/
{
    private readonly ITenant _db;
    private readonly IMemoryCache _memoryCache;
    //private readonly TenantOptions _tenantOptions;

    private TenantManager(ISqlSugarClient db, IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        //_tenantOptions = tenantOptions.Value;
        _db = db.AsTenant();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <param name="db"></param>
    /// <param name="memoryCache"></param>
    public TenantManager(IHttpContextAccessor httpContextAccessor, ISqlSugarClient db, IMemoryCache memoryCache) : this(db, memoryCache)
    {
        if (httpContextAccessor != null && httpContextAccessor.HttpContext != null && KeyVariable.MultiTenancy)
        {
            string tenantId = TenantScoped.TenantId ?? string.Empty;

            // 优先判断请求头
            if (string.IsNullOrEmpty(tenantId))
            {
                var str = httpContextAccessor?.HttpContext?.Request.Headers["x-saas-token"];

                if (!string.IsNullOrEmpty(str))
                {
                    var tenantList = _memoryCache.Get<List<ITenantDbInfo>>("tenant:dbcontext:list");
                    if (tenantList.IsAny() && tenantList.Any(x => x.enCode == str.ToString()))
                    {
                        tenantId = str.ToString();
                    }
                    else
                    {
                        var decrypt = DESCEncryption.Decrypt(str, "QT");
                        var model = JSON.Deserialize<TenantScopeModel>(decrypt);
                        tenantId = model?.Code;
                    }


                }
            }
            if (string.IsNullOrEmpty(tenantId))
            {
                tenantId = httpContextAccessor?.HttpContext?.User.FindFirst(ClaimConst.TENANTID)?.Value;
            }

            if (!string.IsNullOrEmpty(tenantId))
            {
                try
                {
                    Login(tenantId);

                    //// 创建租户初始化器
                    //var t = TenantCacheFactory.GetOrCreate(typeof(ITenantInitializer<>), tenantId);
                    //if (t!=null)
                    //{
                    //    _ = App.GetService(t);
                    //}
                }
                catch (AppFriendlyException ex)
                {
                    //var httpContext = httpContextAccessor?.HttpContext;
                    //if (httpContext!=null)
                    //{
                    //    if (!httpContext.Response.HasStarted)
                    //    {
                    //        httpContext.Response.StatusCode = 400; // 设置状态码
                    //        httpContext.Response.ContentType = "application/json";
                    //        httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new { code = 400, message = ex.Message ?? "租户数据库登录失败" }));
                    //        httpContext.Response.CompleteAsync().GetAwaiter().GetResult(); // 结束响应
                    //        //return;
                    //    }
                    //}
                    throw ex;
                    //throw new Exception(ex.Message ?? "租户数据库登录失败");
                }
            }

        }
    }

    /// <summary>
    /// 租户是否登录（是否设置了数据库链接）
    /// </summary>
    public bool IsLoggedIn
    {
        get
        {
            if (!string.IsNullOrEmpty(this.TenantId) && _db.IsAnyConnection(this.TenantId))
            {
                _db.ChangeDatabase(this.TenantId);
                return true;
            }
            return false;
        }
    }

    private string _tenantId = string.Empty;

    /// <summary>
    /// 租户编号
    /// </summary>
    public string TenantId => _tenantId;

    /// <summary>
    /// 租户登录，设置租户数据库链接.
    /// </summary>
    /// <param name="tenantId"></param>
    /// <exception cref="AppFriendlyException"></exception>
    protected virtual void Login(string tenantId)
    {
        if (!KeyVariable.MultiTenancy)
        {
            return;
        }
        if (string.IsNullOrEmpty(tenantId))
        {
            var ex = Oops.Oh(ErrorCode.Zh10000);
            ex.StatusCode = 610001;
            throw ex;
        }
        this._tenantId = tenantId;

        if (this.IsLoggedIn)
        {
            return;
        }

        var linkList = _memoryCache.GetOrCreate($"saas:id:{tenantId}", (entry) =>
        {
            List<TenantLinkModel>? linkList = new List<TenantLinkModel>();
            var interFace = string.Format("{0}{1}", KeyVariable.MultiTenancyDBInterFace, tenantId);

            //interFace.set
            var response = interFace.GetAsStringAsync().GetAwaiter().GetResult();
            var result = response.ToObject<RESTfulResult<TenantInterFaceOutput>>();
            if (result.code != 200)
            {
                var ex = Oops.Oh(result.msg?.ToString() ?? "单位不存在");
                ex.StatusCode = 610001;
                throw ex;
                //throw new AppFriendlyException("单位不存在", 610001);
            }

            //throw Oops.Oh(result.msg);
            else if (result.data.dotnet == null)
            {
                var ex = Oops.Oh("单位登录失败");
                ex.StatusCode = 610001;
                throw ex;
            }
            //throw new AppFriendlyException("单位登录失败", 610001);
            //throw Oops.Oh(ErrorCode.D1025);

            TenantScopeModel tenantScopeModel = result.data.Adapt<TenantScopeModel>();
            if (!tenantScopeModel.linkList.IsAny())
            {
                var ex = Oops.Oh("单位数据库不存在！");
                ex.StatusCode = 610001;
                throw ex;
                //throw new AppFriendlyException("单位数据库不存在！", 610001);
            }
            linkList = tenantScopeModel.linkList;
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
            return linkList;
        });

        // 判断是否启用独立日志库
        var logLinkList = linkList.Where(x => x.configType == 2).ToList();
        if (logLinkList.IsAny())
        {
            linkList = linkList.Except(logLinkList).ToList();
            var logdb = new TenantListInterFaceOutput();

            var configId = $"{tenantId}_log";

            if (!_db.IsAnyConnection(configId))
            {
                var connectionConfig = SqlSugarHelper.CreateConnectionConfig(configId, new TenantInterFaceOutput
                {
                    linkList = logLinkList.Take(1).ToList()
                });
                _db.AddConnection(connectionConfig);
            }

        }

        // 创建sqlsugar
        if (!_db.IsAnyConnection(tenantId) && linkList.IsAny())
        {
            var connectionConfig = SqlSugarHelper.CreateConnectionConfig(tenantId, new TenantInterFaceOutput
            {
                linkList = linkList
            });
            _db.AddConnection(connectionConfig);
            var newDb = _db.GetConnection(tenantId);
            InitDatabase(newDb);
        }
        _db.ChangeDatabase(tenantId);

        if (App.HttpContext != null && !App.HttpContext.Items.ContainsKey(ClaimConst.TENANTID))
        {
            App.HttpContext.Items[ClaimConst.TENANTID] = tenantId;
        }
    }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    protected void InitDatabase(SqlSugarProvider context)
    {
        context.InitDb();

        #region 旧的方法
        //        #region AOP拦截
        //        var sqlSugarInsertByObjects = App.RootServices?.GetServices<ISqlSugarInsertByObject>();
        //        var sqlSugarUpdateByObjects = App.RootServices?.GetServices<ISqlSugarUpdateByObject>();
        //        var sqlSugarDeleteByObjects = App.RootServices?.GetServices<ISqlSugarDeleteByObject>();
        //        context.Aop.DataExecuting = (oldValue, entityInfo) =>
        //        {
        //            //Console.WriteLine("Tenant:{0}", context.CurrentConnectionConfig.ConfigId);
        //            if (KeyVariable.MultiTenancy)
        //            {
        //#if DEBUG
        //                // 1开头的租户是正式账套，不允许新增删除
        //                if (context.CurrentConnectionConfig.ConfigId != null && context.CurrentConnectionConfig.ConfigId!.ToString()!.StartsWith("1"))
        //                {
        //                    if (!entityInfo.EntityColumnInfo.DbTableName.StartsWith("base_",StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        throw Oops.Oh("当前单位编号【{0}】，不允许新增、修改、删除数据！", this.TenantId);
        //                    }                    
        //                }
        //#endif
        //            }
        //            if (entityInfo.OperationType == DataFilterType.InsertByObject)
        //            {
        //                if (entityInfo.PropertyName == "CreatorTime")
        //                    entityInfo.SetValue(DateTime.Now);
        //                if (App.User != null)
        //                {
        //                    if (entityInfo.PropertyName == "CreatorUserId")
        //                    {
        //                        var creatorUserId = ((dynamic)entityInfo.EntityValue).CreatorUserId;
        //                        if (string.IsNullOrEmpty(creatorUserId))
        //                            entityInfo.SetValue(App.User.FindFirst(ClaimConst.CLAINMUSERID)?.Value);
        //                    }
        //                }

        //                if (sqlSugarInsertByObjects != null && sqlSugarInsertByObjects.IsAny())
        //                {
        //                    foreach (var item in sqlSugarInsertByObjects)
        //                    {
        //                        item.Execute(entityInfo);
        //                    }
        //                }
        //            }
        //            if (entityInfo.OperationType == DataFilterType.UpdateByObject)
        //            {
        //                if (entityInfo.PropertyName == "LastModifyTime")
        //                    entityInfo.SetValue(DateTime.Now);
        //                if (entityInfo.PropertyName == "LastModifyUserId")
        //                    entityInfo.SetValue(App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value);

        //                if (sqlSugarUpdateByObjects != null && sqlSugarUpdateByObjects.IsAny())
        //                {
        //                    foreach (var item in sqlSugarUpdateByObjects)
        //                    {
        //                        item.Execute(entityInfo);
        //                    }
        //                }
        //            }
        //            if (entityInfo.OperationType == DataFilterType.DeleteByObject)
        //            {
        //                if (sqlSugarDeleteByObjects != null && sqlSugarDeleteByObjects.IsAny())
        //                {
        //                    foreach (var item in sqlSugarDeleteByObjects)
        //                    {
        //                        item.Execute(entityInfo);
        //                    }
        //                }
        //            }
        //        };
        //        #endregion

        //        var services = App.RootServices?.GetServices<ISqlSugarQueryFilter>();
        //        if (services != null && services.IsAny())
        //        {
        //            foreach (var item in services)
        //            {
        //                item.Execute(context);
        //            }
        //        }

        //        // 开发环境输出sql语句

        //        context.Aop.OnLogExecuting = (sql, pars) =>
        //        {
        //            //Console.WriteLine("Tenant:{0}", context.CurrentConnectionConfig.ConfigId);
        //            if (KeyVariable.MultiTenancy)
        //            {
        //                if (context.CurrentConnectionConfig.ConfigId.ToString() == KeyVariable.DefaultDbConfigId)
        //                {
        //                    throw Oops.Oh("当前单位编号【{0}】", this.TenantId);
        //                }
        //            }
        //#if DEBUG
        //            if (sql.StartsWith("SELECT"))
        //                Console.ForegroundColor = ConsoleColor.Green;

        //            if (sql.StartsWith("UPDATE") || sql.StartsWith("INSERT"))
        //                Console.ForegroundColor = ConsoleColor.White;

        //            if (sql.StartsWith("DELETE"))
        //                Console.ForegroundColor = ConsoleColor.Blue;


        //            // 在控制台输出sql语句
        //            Console.WriteLine($"{context.CurrentConnectionConfig.ConfigId}：" +SqlProfiler.ParameterFormat(sql, pars));
        //            Console.WriteLine();

        //            // 在MiniProfiler内显示
        //            // App.PrintToMiniProfiler("SqlSugar", "Info", SqlProfiler.ParameterFormat(sql, pars));

        //#endif
        //        };

        //        // 监控所有超过1秒的Sql
        //        context.Aop.OnLogExecuted = (sql, pars) =>
        //        {
        //            //执行时间超过1秒
        //            if (context.Ado.SqlExecutionTime.TotalSeconds > 0.1)
        //            {
        //                //代码CS文件名
        //                var fileName = context.Ado.SqlStackTrace.FirstFileName;
        //                //代码行数
        //                var fileLine = context.Ado.SqlStackTrace.FirstLine;
        //                //方法名
        //                var FirstMethodName = context.Ado.SqlStackTrace.FirstMethodName;
        //                //db.Ado.SqlStackTrace.MyStackTraceList[1].xxx 获取上层方法的信息

        //                Console.ForegroundColor = ConsoleColor.Red;
        //                Console.WriteLine($"SQL执行超过0.1秒：[CS文件：{fileName}]-[位置：{fileLine}]-[方法名：{FirstMethodName}]-[执行耗时：{context.Ado.SqlExecutionTime.TotalSeconds}]");
        //                Console.WriteLine(SqlProfiler.ParameterFormat(sql, pars));
        //                Console.WriteLine("###############################");
        //            }
        //            //相当于EF的 PrintToMiniProfiler
        //        };

        //        /*
        //         * 执行之前需要调用 EnableDiffLogEvent
        //         * */
        //        context.Aop.OnDiffLogEvent = (it) =>
        //        {
        //            //操作前记录  包含： 字段描述 列名 值 表名 表描述
        //            var editBeforeData = it.BeforeData;//插入Before为null，之前还没进库
        //                                               //操作后记录   包含： 字段描述 列名 值  表名 表描述
        //            var editAfterData = it.AfterData;
        //            var sql = it.Sql;
        //            var parameter = it.Parameters;
        //            var data = it.BusinessData;//这边会显示你传进来的对象
        //            var time = it.Time;
        //            var diffType = it.DiffType;//enum insert 、update and delete 

        //            //if (it.DiffType == DiffType.update)
        //            //{
        //            //    List<dynamic> list = new List<dynamic>();
        //            //    foreach (var table in editAfterData)
        //            //    {
        //            //        var beforeTable = editBeforeData?.Find(x => x.TableName == table.TableName);

        //            //        foreach (var column in table.Columns)
        //            //        {
        //            //            var oldColumn = beforeTable?.Columns?.Find(x => x.ColumnName == column.ColumnName);

        //            //            if (oldColumn == null || (oldColumn.Value==null && column.Value!=null) || (oldColumn.Value != null && column.Value == null) || (!oldColumn.Value.Equals(column.Value)))
        //            //            {
        //            //                var log = new
        //            //                {
        //            //                    tableName = table.TableName,
        //            //                    fieldName = column.ColumnName,
        //            //                    title = column.ColumnDescription ?? column.ColumnName,
        //            //                    oldValue = oldColumn?.Value,
        //            //                    newValue = column.Value
        //            //                };
        //            //                list.Add(log);
        //            //            }

        //            //        }
        //            //    }
        //            //}

        //            var logDiff = new SysLogDiff
        //            {
        //                Id = SnowflakeIdHelper.NextId(),
        //                // 操作后记录（字段描述、列名、值、表名、表描述）
        //                AfterData = JSON.Serialize(editAfterData),
        //                // 操作前记录（字段描述、列名、值、表名、表描述）
        //                BeforeData = JSON.Serialize(editBeforeData),
        //                // 传进来的对象
        //                BusinessData = JSON.Serialize(data),
        //                // 枚举（insert、update、delete）
        //                DiffType = diffType.ToString(),
        //                Sql = UtilMethods.GetSqlString(context.CurrentConnectionConfig.DbType, sql, parameter),
        //                Parameters = JSON.Serialize(parameter),
        //                Elapsed = time == null ? 0 : (long)time.Value.TotalMilliseconds,
        //                CreateTime = DateTime.Now,
        //                CreateUserId = App.User.FindFirst(ClaimConst.CLAINMUSERID)?.Value ?? ""
        //            };

        //            var tenantId = context.CurrentConnectionConfig.ConfigId.ToString();
        //            App.GetService<IEventPublisher>()?.PublishAsync(new LogEventSource<SysLogDiff>("Log:CreateDiffLog", tenantId, tenantId, logDiff));
        //            context.Insertable<SysLogDiff>(logDiff).ExecuteCommand();
        //        }; 
        #endregion
    }

    /// <summary>
    /// 租户登录，注册数据库链接
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    public static bool Login([NotNull] string tenantId, [NotNull] ISqlSugarClient db)
    {
        var manager = new TenantManager(db, App.GetService<IMemoryCache>());
        manager._tenantId = tenantId;
        if (manager.IsLoggedIn)
        {
            return true;
        }
        try
        {
            manager.Login(tenantId);
        }
        catch (Exception ex)
        {
            return false;
        }

        return manager.IsLoggedIn;
    }

    public ITenantDbInfo? GetTenantInfo()
    {
        var tenantList = _memoryCache.Get<List<ITenantDbInfo>>("tenant:dbcontext:list");
        return tenantList?.FirstOrDefault(x => x.enCode == TenantId);
    }
}
