using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QT.Common.Cache;
using QT.Common.Const;
using QT.Common.Core.Manager;

namespace QT.Common.Core.Filter;

/// <summary>
/// 缓存资源过滤器
/// </summary>
//[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class CachingFilterAttribute : Attribute, IAsyncResourceFilter
{
    /// <summary>
    /// 过期时间(秒)
    /// </summary>
    private readonly int Expired;

    /// <summary>
    /// 是否区分当前用户
    /// </summary>
    private readonly bool IsAuth;
    public CachingFilterAttribute(int expired = 60, bool isAuth = false)
    {
        this.Expired = expired;
        this.IsAuth = isAuth;
    }

    /// <summary>
    /// 获取缓存
    /// </summary>
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        ////检查是否启用Redis
        //Boolean.TryParse(Appsettings.GetValue(new string[] { "CacheSettings", "RedisServer", "Enabled" }), out bool redisEnabled);
        ////检查是否启用MemoryCache
        //Boolean.TryParse(Appsettings.GetValue(new string[] { "CacheSettings", "MemoryCaching", "Enabled" }), out bool memoryEnabled);

        //获取缓存Key
        var cacheKey = $"{context.HttpContext.Request.Host}{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}";
        //区分当前用户存储
        if (IsAuth)
        {
            var httpContextAccessor = context.HttpContext.RequestServices.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            var currUser = httpContextAccessor?.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (currUser != null)
            {
                cacheKey = $"{cacheKey}:{currUser.Value}";
            }
        }
        var cacheManager = context.HttpContext.RequestServices.GetService(typeof(ICacheManager)) as ICacheManager;
        if (cacheManager!=null)
        {
            var cacheResult = cacheManager.Get<CacheResult>(cacheKey);
            if (cacheResult != null)
            {
                foreach (var item in cacheResult.Headers)
                {
                    context.HttpContext.Response.Headers.Add(item.Key, item.Value);
                }
                context.Result = cacheResult.Result;
            }
        }
    }

    /// <summary>
    /// 写入缓存
    /// </summary>
    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        ////检查是否启用Redis
        //Boolean.TryParse(Appsettings.GetValue(new string[] { "CacheSettings", "RedisServer", "Enabled" }), out bool redisEnabled);
        ////检查是否启用MemoryCache
        //Boolean.TryParse(Appsettings.GetValue(new string[] { "CacheSettings", "MemoryCaching", "Enabled" }), out bool memoryEnabled);
        //获取缓存Key
        var cacheKey = $"{context.HttpContext.Request.Host}{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}";
        //区分当前用户存储
        if (IsAuth)
        {
            var httpContextAccessor = context.HttpContext.RequestServices.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            var currUser = httpContextAccessor?.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (currUser != null)
            {
                cacheKey = $"{cacheKey}:{currUser.Value}";
            }
        }
        var cacheManager = context.HttpContext.RequestServices.GetService(typeof(ICacheManager)) as ICacheManager;
        if (cacheManager!=null)
        {
            //有结果则写入缓存
            if (context.Result is ObjectResult objResult && objResult.StatusCode == StatusCodes.Status200OK)
            {
                var cacheResult = new CacheResult();
                if (context.HttpContext.Response.Headers.ContainsKey("x-pagination"))
                {
                    cacheResult.Headers.Add("x-pagination", context.HttpContext.Response.Headers["x-pagination"]);
                }
                cacheResult.Result = objResult;
                cacheManager.Set(cacheKey, cacheResult, TimeSpan.FromSeconds(Expired));
            }
        }
        ////Redis缓存
        //if (redisEnabled)
        //{
        //    if (context.HttpContext.RequestServices.GetService(typeof(IRedisCacheManager)) is IRedisCacheManager redisCache)
        //    {
        //        //有结果则写入缓存
        //        if (context.Result is ObjectResult objResult && objResult.StatusCode == StatusCodes.Status200OK)
        //        {
        //            var cacheResult = new CacheResult();
        //            if (context.HttpContext.Response.Headers.ContainsKey("x-pagination"))
        //            {
        //                cacheResult.Headers.Add("x-pagination", context.HttpContext.Response.Headers["x-pagination"]);
        //            }
        //            cacheResult.Result = objResult;
        //            redisCache.Set(cacheKey, cacheResult, TimeSpan.FromSeconds(Expired));
        //        }
        //    }
        //}
        ////MemoryCache
        //if (memoryEnabled)
        //{
        //    var objResult = context.Result as ObjectResult; //有结果则写入缓存
        //    if (objResult != null && objResult.StatusCode == StatusCodes.Status200OK)
        //    {
        //        var cacheResult = new CacheResult();
        //        if (context.HttpContext.Response.Headers.ContainsKey("x-pagination"))
        //        {
        //            cacheResult.Headers.Add("x-pagination", context.HttpContext.Response.Headers["x-pagination"]);
        //        }
        //        cacheResult.Result = objResult;
        //        MemoryCacheHelper.Set(cacheKey, cacheResult, TimeSpan.FromSeconds(Expired));
        //    }
        //}

    }

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        //获取缓存Key
        var cacheKey = $"{context.HttpContext.Request.Host}{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}";
        //区分当前用户存储
        if (IsAuth)
        {
            var httpContextAccessor = context.HttpContext.RequestServices.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            var currUser = httpContextAccessor?.HttpContext?.User.FindFirst(ClaimConst.CLAINMUSERID);
            if (currUser != null)
            {
                cacheKey = $"{cacheKey}:{currUser.Value}";
            }
        }
        var cacheManager = context.HttpContext.RequestServices.GetService(typeof(ICacheManager)) as ICacheManager;
        if (cacheManager == null)
        {
            await next();
            return;
        }
        var cacheResult = await cacheManager.GetAsync<CacheResult>(cacheKey);
        if (cacheResult != null)
        {
            foreach (var item in cacheResult.Headers)
            {
                context.HttpContext.Response.Headers.Add(item.Key, item.Value);
            }
            context.Result = cacheResult.Result;
            return;
        }
        else
        {
            var actionExecutedContext = await next();
             
            ////////// 写入缓存 //////////////
            //有结果则写入缓存
            if (actionExecutedContext.Result is ObjectResult objResult && (objResult.StatusCode == StatusCodes.Status200OK || context.HttpContext.Response.StatusCode == StatusCodes.Status200OK))
            {
                cacheResult = new CacheResult();
                if (context.HttpContext.Response.Headers.ContainsKey("x-pagination"))
                {
                    cacheResult.Headers.Add("x-pagination", context.HttpContext.Response.Headers["x-pagination"]);
                }
                cacheResult.Result = objResult;
                cacheManager.Set(cacheKey, cacheResult, TimeSpan.FromSeconds(Expired));
            }
        }
    }
}

/// <summary>
/// 缓存结果
/// </summary>
public class CacheResult
{
    public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    public ObjectResult? Result { get; set; }
}
