using System.Linq.Expressions;
using System.Text.RegularExpressions;
using QT.DataEncryption;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace SqlSugar;

/// <summary>
/// SqlSugar 仓储实现类
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public partial class SqlSugarRepository<TEntity> : ISqlSugarRepository<TEntity>
where TEntity : class, new()
{
    /// <summary>
    /// 初始化 SqlSugar 客户端
    /// </summary>
    private readonly SqlSugarClient _db;

    /// <summary>
    /// 全局配置选项
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// 租户ID或者数据库连接ID
    /// </summary>
    private readonly string _tenantId;

    /// <summary>
    /// 根服务
    /// </summary>
    private readonly IServiceProvider _rootServices;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="services"></param>
    /// <param name="context"></param>
    public SqlSugarRepository(IServiceProvider services, ISqlSugarClient context)
    {
        _rootServices = services;
        var httpContext = _rootServices?.GetService<IHttpContextAccessor>()?.HttpContext;
        _config = _rootServices?.GetService<IConfiguration>();

        Context = _db = (SqlSugarClient)context;
        if (httpContext?.GetEndpoint()?.Metadata?.GetMetadata<AllowAnonymousAttribute>() == null 
            || !string.IsNullOrEmpty(httpContext?.Request.Query["token"]) 
            || !string.IsNullOrEmpty(httpContext?.Request.Headers["Authorization"])
            || !string.IsNullOrEmpty(httpContext?.Request.Headers["X-Saas-Token"]))
        {
            _tenantId = _config["ConnectionStrings:ConfigId"];
            if (_config["Tenant:MultiTenancy"] == "True" && httpContext != null)
            {
                _tenantId = httpContext?.User.FindFirst("TenantId")?.Value;
                //var tenantDbName = httpContext?.User.FindFirst("TenantDbName")?.Value;
                if (!httpContext.Request.Headers.ContainsKey("Authorization"))
                {
                    var token = Regex.Match(httpContext.Request.QueryString.Value, @"[?&]token=Bearer%20([\w\.-]+)($|&)").Groups[1].Value;
                    var claims = JWTEncryption.ReadJwtToken(token.Replace("Bearer ", "").Replace("bearer ", ""))?.Claims;
                    _tenantId = claims?.FirstOrDefault(e => e.Type == "TenantId")?.Value;
                    //tenantDbName = claims.FirstOrDefault(e => e.Type == "TenantDbName")?.Value;
                }
                var sugarTenant = _rootServices?.GetService<ISqlSugarTenant>();
                if (sugarTenant == null || !sugarTenant.IsLoggedIn)
                {
                    //var dbType = (DbType)Enum.Parse(typeof(DbType), _config["ConnectionStrings:DBType"]);
                    //if (!Context.IsAnyConnection(_tenantId))
                    //{
                    //    Context.AddConnection(new ConnectionConfig()
                    //    {
                    //        DbType = dbType,
                    //        ConfigId = _tenantId,//设置库的唯一标识
                    //        IsAutoCloseConnection = true,
                    //        ConnectionString = string.Format($"{_config["ConnectionStrings:DefaultConnection"]}", tenantDbName),
                    //        ConfigureExternalServices = new ConfigureExternalServices() { }
                    //    });
                    //}
                    //Context.ChangeDatabase(_tenantId);
                    //var ex = new Exception("租户数据库登录失败");

                    if (httpContext!=null)
                    {
                        //httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                        //httpContext.Response.WriteAsync("租户数据库登录失败").GetAwaiter().GetResult();

                        if (!httpContext.Response.HasStarted)
                        {
                            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest; // 设置状态码
                            httpContext.Response.ContentType = "application/json";
                            httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new { code = 610001, message = "租户数据库登录失败" }));
                            httpContext.Response.CompleteAsync().GetAwaiter().GetResult(); // 结束响应
                        }
                        throw new Exception("租户数据库登录失败");
                        //return;
                    }

                    throw new Exception("租户数据库登录失败");
                }

                _db = Context;

                // 移除默认的数据库连接
                _db.RemoveConnection("default");

                //if (!Context.Ado.IsValidConnection())
                //{
                //    throw new Exception("数据库连接移除");
                //}

                //放到 TenantManager 统一处理
                //Context.Aop.OnLogExecuting = (sql, pars) =>
                //{
                //    if (sql.StartsWith("SELECT"))
                //        Console.ForegroundColor = ConsoleColor.Green;

                //    if (sql.StartsWith("UPDATE") || sql.StartsWith("INSERT"))
                //        Console.ForegroundColor = ConsoleColor.White;

                //    if (sql.StartsWith("DELETE"))
                //        Console.ForegroundColor = ConsoleColor.Blue;

                //    // 在控制台输出sql语句
                //    Console.WriteLine(SqlProfiler.ParameterFormat(sql, pars));
                //    Console.WriteLine();

                //    // 在MiniProfiler内显示
                //    // App.PrintToMiniProfiler("SqlSugar", "Info", SqlProfiler.ParameterFormat(sql, pars));
                //};
            }
        }
        Ado = Context.Ado;
    }

    /// <summary>
    /// 实体集合
    /// </summary>
    public virtual ISugarQueryable<TEntity> Entities => _db.Queryable<TEntity>();

    /// <summary>
    /// 数据库上下文
    /// </summary>
    public virtual SqlSugarClient Context { get; }

    /// <summary>
    /// 原生 Ado 对象
    /// </summary>
    public virtual IAdo Ado { get; }

    /// <summary>
    /// 获取总数
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public int Count(Expression<Func<TEntity, bool>> whereExpression)
    {
        return Entities.Count(whereExpression);
    }

    /// <summary>
    /// 获取总数
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression)
    {
        return Entities.CountAsync(whereExpression);
    }

    /// <summary>
    /// 检查是否存在
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public bool Any(Expression<Func<TEntity, bool>> whereExpression)
    {
        return Entities.Any(whereExpression);
    }

    /// <summary>
    /// 检查是否存在
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> whereExpression)
    {
        return await Entities.AnyAsync(whereExpression);
    }

    /// <summary>
    /// 通过主键获取实体
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    public TEntity Single(dynamic Id)
    {
        return Entities.InSingle(Id);
    }

    /// <summary>
    /// 获取一个实体
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public TEntity Single(Expression<Func<TEntity, bool>> whereExpression)
    {
        return Entities.Single(whereExpression);
    }

    /// <summary>
    /// 获取一个实体
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> whereExpression)
    {
        return Entities.SingleAsync(whereExpression);
    }

    /// <summary>
    /// 获取一个实体
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> whereExpression)
    {
        return Entities.First(whereExpression);
    }

    /// <summary>
    /// 获取一个实体
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> whereExpression)
    {
        return await Entities.FirstAsync(whereExpression);
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <returns></returns>
    public List<TEntity> ToList()
    {
        return Entities.ToList();
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public List<TEntity> ToList(Expression<Func<TEntity, bool>> whereExpression)
    {
        return Entities.Where(whereExpression).ToList();
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="orderByExpression"></param>
    /// <param name="orderByType"></param>
    /// <returns></returns>
    public List<TEntity> ToList(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
    {
        return Entities.OrderByIF(orderByExpression != null, orderByExpression, orderByType).Where(whereExpression).ToList();
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <returns></returns>
    public Task<List<TEntity>> ToListAsync()
    {
        return Entities.ToListAsync();
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> whereExpression)
    {
        return Entities.Where(whereExpression).ToListAsync();
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="orderByExpression"></param>
    /// <param name="orderByType"></param>
    /// <returns></returns>
    public Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
    {
        return Entities.OrderByIF(orderByExpression != null, orderByExpression, orderByType).Where(whereExpression).ToListAsync();
    }

    /// <summary>
    /// 新增一条记录
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual int Insert(TEntity entity)
    {
        return _db.Insertable(entity).ExecuteCommand();
    }

    /// <summary>
    /// 新增多条记录
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual int Insert(params TEntity[] entities)
    {
        return _db.Insertable(entities).ExecuteCommand();
    }

    /// <summary>
    /// 新增多条记录
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual int Insert(IEnumerable<TEntity> entities)
    {
        return _db.Insertable(entities.ToArray()).ExecuteCommand();
    }

    /// <summary>
    /// 新增一条记录返回自增Id
    /// </summary>
    /// <param name="insertObj"></param>
    /// <returns></returns>
    public int InsertReturnIdentity(TEntity insertObj)
    {
        return _db.Insertable(insertObj).ExecuteReturnIdentity();
    }

    /// <summary>
    /// 新增一条记录
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual Task<int> InsertAsync(TEntity entity)
    {
        return _db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 新增多条记录
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual Task<int> InsertAsync(params TEntity[] entities)
    {
        return _db.Insertable(entities).ExecuteCommandAsync();
    }

    /// <summary>
    /// 新增多条记录
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual Task<int> InsertAsync(IEnumerable<TEntity> entities)
    {
        if (entities != null && entities.Any())
        {
            return _db.Insertable(entities.ToArray()).ExecuteCommandAsync();
        }
        return Task.FromResult(0);
    }

    /// <summary>
    /// 新增一条记录返回自增Id
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task<long> InsertReturnIdentityAsync(TEntity entity)
    {
        return await _db.Insertable(entity).ExecuteReturnBigIdentityAsync();
    }



    /// <summary>
    /// 更新一条记录
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual int Update(TEntity entity)
    {
        return _db.Updateable(entity).ExecuteCommand();
    }

    /// <summary>
    /// 更新多条记录
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual int Update(params TEntity[] entities)
    {
        return _db.Updateable(entities).ExecuteCommand();
    }

    /// <summary>
    /// 更新多条记录
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual int Update(IEnumerable<TEntity> entities)
    {
        return _db.Updateable(entities.ToArray()).ExecuteCommand();
    }

    /// <summary>
    /// 更新一条记录
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual Task<int> UpdateAsync(TEntity entity)
    {
        return _db.Updateable(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 无主键更新一条记录
    /// </summary>
    /// <param name="entity">更新的实体</param>
    /// <param name="columns">根据那些字段更新</param>
    /// <returns></returns>
    public virtual Task<int> UpdateNoPrimaryKey(TEntity entity, Expression<Func<TEntity, object>> columns)
    {
        return _db.Updateable(entity).WhereColumns(columns).ExecuteCommandAsync();
    }

    /// <summary>
    /// 无主键更新一条记录
    /// </summary>
    /// <param name="entity">更新的实体</param>
    /// <param name="columns">根据那些字段更新</param>
    /// <returns></returns>
    public virtual Task<int> UpdateNoPrimaryKeyAsync(TEntity entity, Expression<Func<TEntity, object>> columns)
    {
        return _db.Updateable(entity).WhereColumns(columns).ExecuteCommandAsync();
    }

    /// <summary>
    /// 无主键更新多条记录
    /// </summary>
    /// <param name="entitys">更新的实体</param>
    /// <param name="columns">根据那些字段更新</param>
    /// <returns></returns>
    public virtual Task<int> UpdateNoPrimaryKey(List<TEntity> entitys, Expression<Func<TEntity, object>> columns)
    {
        return _db.Updateable(entitys).WhereColumns(columns).ExecuteCommandAsync();
    }

    /// <summary>
    /// 无主键更新多条记录
    /// </summary>
    /// <param name="entitys">更新的实体</param>
    /// <param name="columns">根据那些字段更新</param>
    /// <returns></returns>
    public virtual Task<int> UpdateNoPrimaryKeyAsync(List<TEntity> entitys, Expression<Func<TEntity, object>> columns)
    {
        return _db.Updateable(entitys).WhereColumns(columns).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新多条记录
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual Task<int> UpdateAsync(params TEntity[] entities)
    {
        return _db.Updateable(entities).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新多条记录
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual Task<int> UpdateAsync(IEnumerable<TEntity> entities)
    {
        return _db.Updateable(entities.ToArray()).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除一条记录
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual int Delete(TEntity entity)
    {
        return _db.Deleteable(entity).ExecuteCommand();
    }

    /// <summary>
    /// 删除一条记录
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public virtual int Delete(object key)
    {
        return _db.Deleteable<TEntity>().In(key).ExecuteCommand();
    }

    /// <summary>
    /// 删除多条记录
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    public virtual int Delete(params object[] keys)
    {
        return _db.Deleteable<TEntity>().In(keys).ExecuteCommand();
    }

    /// <summary>
    /// 自定义条件删除记录
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public int Delete(Expression<Func<TEntity, bool>> whereExpression)
    {
        return _db.Deleteable<TEntity>().Where(whereExpression).ExecuteCommand();
    }

    /// <summary>
    /// 删除一条记录
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual Task<int> DeleteAsync(TEntity entity)
    {
        return _db.Deleteable(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除一条记录
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public virtual Task<int> DeleteAsync(object key)
    {
        return _db.Deleteable<TEntity>().In(key).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除多条记录
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    public virtual Task<int> DeleteAsync(params object[] keys)
    {
        return _db.Deleteable<TEntity>().In(keys).ExecuteCommandAsync();
    }

    /// <summary>
    /// 自定义条件删除记录
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression)
    {
        return await _db.Deleteable<TEntity>().Where(whereExpression).ExecuteCommandAsync();
    }

    /// <summary>
    /// 根据表达式查询多条记录
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public virtual ISugarQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        return AsQueryable(predicate);
    }

    /// <summary>
    /// 根据表达式查询多条记录
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public virtual ISugarQueryable<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> predicate)
    {
        return AsQueryable().WhereIF(condition, predicate);
    }

    /// <summary>
    /// 构建查询分析器
    /// </summary>
    /// <returns></returns>
    public virtual ISugarQueryable<TEntity> AsQueryable()
    {
        return Entities;
    }

    /// <summary>
    /// 构建查询分析器
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public virtual ISugarQueryable<TEntity> AsQueryable(Expression<Func<TEntity, bool>> predicate)
    {
        return Entities.Where(predicate);
    }

    /// <summary>
    /// 直接返回数据库结果
    /// </summary>
    /// <returns></returns>
    public virtual List<TEntity> AsEnumerable()
    {
        return AsQueryable().ToList();
    }

    /// <summary>
    /// 直接返回数据库结果
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public virtual List<TEntity> AsEnumerable(Expression<Func<TEntity, bool>> predicate)
    {
        return AsQueryable(predicate).ToList();
    }

    /// <summary>
    /// 直接返回数据库结果
    /// </summary>
    /// <returns></returns>
    public virtual Task<List<TEntity>> AsAsyncEnumerable()
    {
        return AsQueryable().ToListAsync();
    }

    /// <summary>
    /// 直接返回数据库结果
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public virtual Task<List<TEntity>> AsAsyncEnumerable(Expression<Func<TEntity, bool>> predicate)
    {
        return AsQueryable(predicate).ToListAsync();
    }
}