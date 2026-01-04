using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NPOI.POIFS.Crypt.Dsig;
using QT;
using QT.Common;
using QT.Common.Configuration;
using QT.Common.Const;
using QT.Common.Dtos.DataBase;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.EventBus;
using QT.Extras.DatabaseAccessor.SqlSugar;
using QT.FriendlyException;
using QT.JsonSerialization;
using QT.Reflection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Yitter.IdGenerator;

namespace SqlSugar;

/// <summary>
/// 
/// </summary>
public static partial class QTSqlSugarExtensions
{
    private static readonly object _debug_sql_lock = new object();

    private static IEventPublisher _eventPublisher = App.GetService<IEventPublisher>();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_db"></param>
    /// <param name="action"></param>
    public static void TranExecute(this ITenant _db,Action action)
    {
        try
        {
            _db.BeginTran();

            action();

            _db.CommitTran();
        }
        catch (Exception ex)
        {
            // 回滚事务
            _db.RollbackTran();
            if (ex is AppFriendlyException appFriendlyException)
            {
                throw appFriendlyException;
            }
            throw Oops.Oh(ErrorCode.COM1001);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_db"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task TranExecute(this ITenant _db, Func<Task> action)
    {
        try
        {
            _db.BeginTran();

            await action();

            _db.CommitTran();
        }
        catch (Exception ex)
        {
            // 回滚事务
            _db.RollbackTran();
            if (ex is AppFriendlyException appFriendlyException)
            {
                throw appFriendlyException;
            }
            throw Oops.Oh(ErrorCode.COM1001);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static List<string>? GetChanges<T>(this SqlSugarClient db,T entity) where T : class, new()
    {
        List<string> changes = new List<string>();
        if (entity == null)
        {
            return null;
        }
        string key = "Tracking_" + entity.GetHashCode();
        if (!db.TempItems.TryGetValue(key, out object? oldData) || oldData ==null || !(oldData is T oldEntity))
        {
            return null;
        }

        PropertyInfo[] properties = EntityHelper<T>.InstanceProperties.ToArray();
        foreach (PropertyInfo propertyInfo in properties)
        {
            object? value = propertyInfo.GetValue(entity);
            object? value2 = propertyInfo.GetValue(oldEntity);
            if (value == null && value2 != null)
            {
                changes.Add(propertyInfo.Name);
            }
            else if (value != null && !value.Equals(value2))
            {
                changes.Add(propertyInfo.Name);
            }
        }

        return changes;
    }
    public static List<FieldChangeDto>? GetChangeFields<T>(this SqlSugarClient db, T entity) where T : class, new()
    {
        
        List<FieldChangeDto> changes = new List<FieldChangeDto>();
        if (entity == null)
        {
            return null;
        }
        string key = "Tracking_" + entity.GetHashCode();
        if (!db.TempItems.TryGetValue(key, out object? oldData) || oldData == null || !(oldData is T oldEntity))
        {
            return null;
        }
        var tableName = typeof(T).Name;
        PropertyInfo[] properties = EntityHelper<T>.InstanceProperties.ToArray();
        foreach (PropertyInfo propertyInfo in properties)
        {
            object? value = propertyInfo.GetValue(entity);
            object? value2 = propertyInfo.GetValue(oldEntity);
            if (value == null && value2 != null)
            {
                FieldChangeDto fieldChangeDto = new FieldChangeDto
                {
                    fieldName = propertyInfo.Name,
                    description = propertyInfo.GetCustomAttribute<SugarColumn>()?.ColumnDescription ?? propertyInfo.GetDescription(),
                    oldValue = value2?.ToString(),
                    newValue = value?.ToString(),
                    tableName = tableName
                };
                changes.Add(fieldChangeDto);
            }
            else if (value != null && !value.Equals(value2))
            {
                FieldChangeDto fieldChangeDto = new FieldChangeDto
                {
                    fieldName = propertyInfo.Name,
                    description = propertyInfo.GetCustomAttribute<SugarColumn>()?.ColumnDescription ?? propertyInfo.GetDescription(),
                    oldValue = value2?.ToString(),
                    newValue = value?.ToString(),
                    tableName = tableName
                };
                changes.Add(fieldChangeDto);
            }
        }

        return changes;
    }

    /// <summary>
    /// 初始化数据库的配置
    /// </summary>
    /// <param name="db"></param>
    public static void InitDb(this ISqlSugarClient db)
    {
        #region AOP拦截
        var sqlSugarInsertByObjects = App.RootServices?.GetServices<ISqlSugarInsertByObject>();
        var sqlSugarUpdateByObjects = App.RootServices?.GetServices<ISqlSugarUpdateByObject>();
        var sqlSugarDeleteByObjects = App.RootServices?.GetServices<ISqlSugarDeleteByObject>();
        db.Aop.DataExecuting = (oldValue, entityInfo) =>
        {
            //Console.WriteLine("Tenant:{0}", context.CurrentConnectionConfig.ConfigId);
            if (KeyVariable.MultiTenancy)
            {
#if DEBUG
                // 1开头的租户是正式账套，不允许新增删除
                if (db.CurrentConnectionConfig.ConfigId != null && db.CurrentConnectionConfig.ConfigId!.ToString()!.StartsWith("1"))
                {
                    if (!entityInfo.EntityColumnInfo.DbTableName.StartsWith("base_", StringComparison.OrdinalIgnoreCase))
                    {
                        //throw Oops.Oh("当前单位编号【{0}】，不允许新增、修改、删除数据！", db.CurrentConnectionConfig.ConfigId);
                    }
                }
#endif
            }
            if (App.HttpContext != null && App.HttpContext.Items.ContainsKey(CommonConst.LoginUserDisableChangeDatabase))
            {
                throw Oops.Oh("当前用户没有权限进行操作！");
            }
            if (entityInfo.OperationType == DataFilterType.InsertByObject)
            {
                if (entityInfo.PropertyName == "CreatorTime")
                    entityInfo.SetValue(DateTime.Now);
                if (App.User != null)
                {
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
                if (sqlSugarDeleteByObjects != null && sqlSugarDeleteByObjects.IsAny())
                {
                    foreach (var item in sqlSugarDeleteByObjects)
                    {
                        item.Execute(entityInfo);
                    }
                }
            }
        };
        #endregion

        var services = App.RootServices?.GetServices<ISqlSugarQueryFilter>();
        if (services != null && services.IsAny())
        {
            foreach (var item in services)
            {
                item.Execute(db);
            }
        }

        // 开发环境输出sql语句

        db.Aop.OnLogExecuting = (sql, pars) =>
        {
            //Console.WriteLine("Tenant:{0}", context.CurrentConnectionConfig.ConfigId);
            if (KeyVariable.MultiTenancy)
            {
                if (db.CurrentConnectionConfig.ConfigId.ToString() == KeyVariable.DefaultDbConfigId)
                {
                    throw Oops.Oh("当前单位编号【{0}】", KeyVariable.DefaultDbConfigId);
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
            //Console.WriteLine($"{db.CurrentConnectionConfig.ConnectionString}");
            Console.WriteLine($"{db.CurrentConnectionConfig.ConfigId}：" + SqlProfiler.ParameterFormat(sql, pars));

            // 在MiniProfiler内显示
            // App.PrintToMiniProfiler("SqlSugar", "Info", SqlProfiler.ParameterFormat(sql, pars));

#endif
        };

        // 监控所有超过1秒的Sql
        db.Aop.OnLogExecuted = (sql, pars) =>
        {
            //var debugs = db.DataCache.Get<List<string>>("DEBUG");
            //if (debugs!=null)
            //{
            //    debugs.Add($"[{db.Ado.SqlExecutionTime.TotalSeconds*1000}]{SqlProfiler.ParameterFormat(sql, pars)}");
            //    db.DataCache.Add("DEBUG", debugs);
            //}

            if (App.HttpContext!=null && App.HttpContext.Items.ContainsKey("DebugScopeSql"))
            {
                db.AddDebugSql($"[{db.Ado.SqlExecutionTime.TotalSeconds}]{SqlProfiler.ParameterFormat(sql, pars)}");
            }
#if DEBUG
            //执行时间超过1秒
            if (db.Ado.SqlExecutionTime.TotalSeconds > 0.1)
            {
                //代码CS文件名
                var fileName = db.Ado.SqlStackTrace.FirstFileName;
                //代码行数
                var fileLine = db.Ado.SqlStackTrace.FirstLine;
                //方法名
                var FirstMethodName = db.Ado.SqlStackTrace.FirstMethodName;
                //db.Ado.SqlStackTrace.MyStackTraceList[1].xxx 获取上层方法的信息

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"SQL执行超过0.1秒：[CS文件：{fileName}]-[位置：{fileLine}]-[方法名：{FirstMethodName}]-[执行耗时：{db.Ado.SqlExecutionTime.TotalSeconds}]");
                Console.WriteLine(SqlProfiler.ParameterFormat(sql, pars));
                Console.WriteLine("###############################");
            }
            //相当于EF的 PrintToMiniProfiler
#endif
        };

        /*
         * 执行之前需要调用 EnableDiffLogEvent
         * */
        db.Aop.OnDiffLogEvent = (it) =>
        {
            //操作前记录  包含： 字段描述 列名 值 表名 表描述
            var editBeforeData = it.BeforeData;//插入Before为null，之前还没进库
                                               //操作后记录   包含： 字段描述 列名 值  表名 表描述
            var editAfterData = it.AfterData;
            var sql = it.Sql;
            var parameter = it.Parameters;
            var data = it.BusinessData;//这边会显示你传进来的对象
            var time = it.Time;
            var diffType = it.DiffType;//enum insert 、update and delete 

            if (data==null)
            {
                data = string.Empty;
            }

            if (data.IsNotEmptyOrNull())
            {
                if (data.GetType() == typeof(string))
                {
                    data = data.ToString();
                }
                else
                {
                    data = JSON.Serialize(data);
                }
                
            }

            //if (it.DiffType == DiffType.update)
            //{
            //    List<dynamic> list = new List<dynamic>();
            //    foreach (var table in editAfterData)
            //    {
            //        var beforeTable = editBeforeData?.Find(x => x.TableName == table.TableName);

            //        foreach (var column in table.Columns)
            //        {
            //            var oldColumn = beforeTable?.Columns?.Find(x => x.ColumnName == column.ColumnName);

            //            if (oldColumn == null || (oldColumn.Value==null && column.Value!=null) || (oldColumn.Value != null && column.Value == null) || (!oldColumn.Value.Equals(column.Value)))
            //            {
            //                var log = new
            //                {
            //                    tableName = table.TableName,
            //                    fieldName = column.ColumnName,
            //                    title = column.ColumnDescription ?? column.ColumnName,
            //                    oldValue = oldColumn?.Value,
            //                    newValue = column.Value
            //                };
            //                list.Add(log);
            //            }

            //        }
            //    }
            //}

            var logDiff = new
            {
                Id = YitIdHelper.NextId(),
                // 操作后记录（字段描述、列名、值、表名、表描述）
                AfterData = JSON.Serialize(editAfterData),
                // 操作前记录（字段描述、列名、值、表名、表描述）
                BeforeData = JSON.Serialize(editBeforeData),
                // 传进来的对象
                BusinessData = data, //JSON.Serialize(data),
                // 枚举（insert、update、delete）
                DiffType = diffType.ToString(),
                Sql = UtilMethods.GetSqlString(db.CurrentConnectionConfig.DbType, sql, parameter),
                Parameters = JSON.Serialize(parameter),
                Elapsed = time == null ? 0 : (long)time.Value.TotalMilliseconds,
                CreateTime = DateTime.Now,
                CreateUserId = App.User.FindFirst(ClaimConst.CLAINMUSERID)?.Value ?? ""
            };

            var tenantId = db.CurrentConnectionConfig.ConfigId.ToString();
            _eventPublisher?.PublishAsync("Log:CreateDiffLog", new
            {
                tenantId,
                entity = logDiff
            }).ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
        };

    }

    /// <summary>
    /// 开启debug模式，监听sql时间
    /// </summary>
    /// <param name="db"></param>
    public static void EnableDebug(this ISqlSugarClient db)
    {
        if (App.HttpContext != null)
        {
            App.HttpContext.Items.TryAdd("DebugScopeSql", new List<string>());
        }
        //var debugs = db.DataCache.Get<List<string>>("DEBUG");
        //if (debugs == null)
        //{
        //    db.DataCache.Add("DEBUG", new List<string>());
        //}        
    }

    /// <summary>
    /// 开启debug模式，监听sql时间
    /// </summary>
    /// <param name="db"></param>
    public static void DisEnableDebug(this ISqlSugarClient db)
    {
        if (App.HttpContext != null)
        {
            App.HttpContext.Items.Remove("DebugScopeSql");
        }       
    }

    /// <summary>
    /// 获取监听的sql时间
    /// </summary>
    /// <param name="db"></param>
    /// <param name="refresh"></param>
    public static List<string> GetDebugSql(this ISqlSugarClient db,bool refresh = true)
    {
        if (App.HttpContext!=null)
        {
            if (App.HttpContext.Items.TryGetValue("DebugScopeSql", out var obj) && obj is List<string> list)
            {
                if (refresh)
                {
                    db.DisEnableDebug();
                }
                return list;
            }
        }
       
        return  new List<string>();
    }

    /// <summary>
    /// 添加调试sql
    /// </summary>
    /// <param name="db"></param>
    /// <param name="sql"></param>
    public static void AddDebugSql(this ISqlSugarClient db, string sql)
    {
        lock (_debug_sql_lock)
        {
            var list = App.HttpContext.Items["DebugScopeSql"] as List<string>;
            if (list != null)
            {
                list.Add(sql);
                App.HttpContext.Items["DebugScopeSql"] = list;

            }
        }
    }

    /// <summary>
    /// 根据当前config，判读是否有日志数据库
    /// 判断条件 {config}_log 是否存在
    /// </summary>
    /// <param name="db"></param>
    public static ISqlSugarClient UseLogDatabase(this SqlSugarClient db)
    {
        var logConfig = $"{db.CurrentConnectionConfig.ConfigId}_log";
        if (!db.IsAnyConnection(logConfig))
        {
            var cache = App.GetRequiredService<IMemoryCache>();
            var logConnection = cache?.Get<List<ConnectionConfig>>("tenant:dbcontext:configs")?.FirstOrDefault(x => x.ConfigId?.ToString() == logConfig);
            if (logConnection != null)
            {
                db.AddConnection(logConnection);
            }
        }
        if (db.IsAnyConnection(logConfig))
        {
            var logDB = db.CopyNew();
            logDB.ChangeDatabase(logConfig);
            return logDB;
        }
        return db;
    }
}
