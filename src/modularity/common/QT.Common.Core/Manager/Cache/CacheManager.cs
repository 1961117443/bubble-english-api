using QT.Common.Cache;
using QT.DependencyInjection;
using Microsoft.Extensions.Options;
using QT.Common.Const;
using QT.Common.Configuration;
using System.Diagnostics;
using QT.Systems.Entitys.Permission;
using SqlSugar;
using Microsoft.Extensions.Caching.Distributed;
using QT.Common.Core.Manager.Tenant;
using Microsoft.Extensions.DependencyInjection;
namespace QT.Common.Core.Manager;

/// <summary>
/// 缓存管理.
/// </summary>
public class CacheManager : ICacheManager, IScoped
{
    private readonly CacheOptions _cacheOptions;
    private readonly ICache _cache;

    /// <summary>
    /// 初始化一个<see cref="CacheManager"/>类型的新实例.
    /// </summary>
    public CacheManager(
        IOptions<CacheOptions> cacheOptions,
        Func<string, ISingleton, object> resolveNamed)
    {
        _cacheOptions = cacheOptions.Value;

        var tenantId = App.HttpContext?.User?.FindFirst(ClaimConst.TENANTID)?.Value ?? App.HttpContext?.Items["TenantId"]?.ToString();
        if (string.IsNullOrEmpty(tenantId))
        {
            tenantId = TenantScoped.TenantId;
        }
        if (!string.IsNullOrEmpty(tenantId) && KeyVariable.MultiTenancy)
        {
            var t = TenantCacheFactory.GetOrCreate(_cacheOptions.CacheType, tenantId);
            _cache = App.GetService(t) as ICache;
        }
        else
        {
            _cache = resolveNamed(_cacheOptions.CacheType.ToString(), default) as ICache;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cache"></param>
    private CacheManager(ICache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cache"></param>
    /// <returns></returns>
    public static ICacheManager CreateInstance(ICache cache)
    {
        return new CacheManager(cache);
    }

    /// <summary>
    /// 获取所有缓存关键字.
    /// </summary>
    /// <returns></returns>
    public List<string> GetAllCacheKeys()
    {
        List<string> cacheItems = _cache.GetAllKeys().ToList();
        if (cacheItems == null) return new List<string>();
        return cacheItems.Where(u => !u.ToString().StartsWith("mini-profiler")).Select(u => u).ToList();
    }

    /// <summary>
    /// 删除指定关键字缓存.
    /// </summary>
    /// <param name="key">键.</param>
    /// <returns></returns>
    public bool Del(string key)
    {
        _cache.Del(key);
        return true;
    }

    /// <summary>
    /// 删除指定关键字缓存.
    /// </summary>
    /// <param name="key">键.</param>
    /// <returns></returns>
    public Task<bool> DelAsync(string key)
    {
        _cache.DelAsync(key);
        return Task.FromResult(true);
    }

    /// <summary>
    /// 删除指定关键字数组缓存.
    /// </summary>
    /// <param name="key">键.</param>
    /// <returns></returns>
    public Task<bool> DelAsync(string[] key)
    {
        _cache.DelAsync(key);
        return Task.FromResult(true);
    }

    /// <summary>
    /// 删除某特征关键字缓存.
    /// </summary>
    /// <param name="key">键.</param>
    /// <returns></returns>
    public Task<bool> DelByPatternAsync(string key)
    {
        _cache.DelByPatternAsync(key);
        return Task.FromResult(true);
    }

    /// <summary>
    /// 设置缓存.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    /// <returns></returns>
    public bool Set(string key, object value)
    {
        return _cache.Set(key, value);
    }

    /// <summary>
    /// 设置缓存.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    /// <param name="timeSpan">过期时间.</param>
    /// <returns></returns>
    public bool Set(string key, object value, TimeSpan timeSpan)
    {
        return _cache.Set(key, value, timeSpan);
    }

    /// <summary>
    /// 设置缓存.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    /// <returns></returns>
    public async Task<bool> SetAsync(string key, object value)
    {
        return await _cache.SetAsync(key, value);
    }

    /// <summary>
    /// 设置缓存.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    /// <param name="timeSpan">过期时间.</param>
    /// <returns></returns>
    public async Task<bool> SetAsync(string key, object value, TimeSpan timeSpan)
    {
        return await _cache.SetAsync(key, value, timeSpan);
    }

    /// <summary>
    /// 获取指定 key 的增量值.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="incrBy">增量.</param>
    /// <returns></returns>
    public long Incrby(string key, long incrBy)
    {
        return _cache.Incrby(key, incrBy);
    }

    /// <summary>
    /// 获取指定 key 的增量值.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="incrBy">增量.</param>
    /// <returns></returns>
    public async Task<long> IncrbyAsync(string key, long incrBy)
    {
        return await _cache.IncrbyAsync(key, incrBy);
    }

    /// <summary>
    /// 获取缓存.
    /// </summary>
    /// <param name="key">键.</param>
    /// <returns></returns>
    public string Get(string key)
    {
        return _cache.Get(key);
    }

    /// <summary>
    /// 获取缓存.
    /// </summary>
    /// <param name="key">键.</param>
    /// <returns></returns>
    public async Task<string> GetAsync(string key)
    {
        return await _cache.GetAsync(key);
    }

    /// <summary>
    /// 获取缓存.
    /// </summary>
    /// <typeparam name="T">对象.</typeparam>
    /// <param name="key">键.</param>
    /// <returns></returns>
    public T Get<T>(string key)
    {
        return _cache.Get<T>(key);
    }

    /// <summary>
    /// 获取缓存.
    /// </summary>
    /// <typeparam name="T">对象.</typeparam>
    /// <param name="key">键.</param>
    /// <returns></returns>
    public Task<T> GetAsync<T>(string key)
    {
        return _cache.GetAsync<T>(key);
    }

    /// <summary>
    /// 获取缓存过期时间.
    /// </summary>
    /// <param name="key">键.</param>
    /// <returns></returns>
    public DateTime GetCacheOutTime(string key)
    {
        return _cache.GetCacheOutTime(key);
    }

    /// <summary>
    /// 检查给定 key 是否存在.
    /// </summary>
    /// <param name="key">键.</param>
    /// <returns></returns>
    public bool Exists(string key)
    {
        return _cache.Exists(key);
    }

    /// <summary>
    /// 检查给定 key 是否存在.
    /// </summary>
    /// <param name="key">键.</param>
    /// <returns></returns>
    public Task<bool> ExistsAsync(string key)
    {
        return _cache.ExistsAsync(key);
    }

    /// <summary>
    /// 只有在 key 不存在时设置 key 的值.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    /// <param name="expire">有效期.</param>
    public bool SetNx(string key, object value, TimeSpan expire)
    {
        return _cache.SetNx(key, value, expire);
    }

    /// <summary>
    /// 只有在 key 不存在时设置 key 的值.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    public bool SetNx(string key, object value)
    {
        return _cache.SetNx(key, value);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<CacheManagerEntryOptions, Task<T>> func)
    {
        var result = await GetAsync<T>(key);
        if (result == null && !await ExistsAsync(key))
        {
            CacheManagerEntryOptions options = new CacheManagerEntryOptions();
            result = await func(options) ?? default(T);
            if (options.AbsoluteExpirationRelativeToNow.TotalSeconds > 0)
            {
                await SetAsync(key, result, options.AbsoluteExpirationRelativeToNow);
            }
            else
            {
                await SetAsync(key, result);
            }
            
        }
        return result;
    }

    public T GetOrCreate<T>(string key, Func<CacheManagerEntryOptions, T> func)
    {
        var result = Get<T>(key);
        if (result == null && !Exists(key))
        {
            CacheManagerEntryOptions options = new CacheManagerEntryOptions();
            result = func(options) ?? default(T);
            if (options.AbsoluteExpirationRelativeToNow.TotalSeconds > 0)
            {
                Set(key, result, options.AbsoluteExpirationRelativeToNow);
            }
            else
            {
                Set(key, result);
            }
                
        }
        return result;
    }
}