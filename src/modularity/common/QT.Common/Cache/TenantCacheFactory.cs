using SqlSugar;
using System.Collections.Concurrent;

namespace QT.Common.Cache;

/// <summary>
/// 缓存工厂
/// </summary>
public static class TenantCacheFactory
{
    /// <summary>
    /// 动态类前缀
    /// </summary>
    public const string DynamicClassPrefix = "tenant_";
    private static readonly ConcurrentDictionary<string, Type> _caches = new ConcurrentDictionary<string, Type>();
    private static object _lock = new object();
    /// <summary>
    /// 获取租户标识类
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public static Type GetOrCreateMark(string tenantId)
    {
        //return typeof(tenant_0001);
        return _caches.GetOrAdd($"{DynamicClassPrefix}{tenantId}", k => DynamicBuilderHelper.CreateDynamicClass(k, new List<PropertyMetadata>()));
    }

    /// <summary>
    /// 获取缓存类
    /// </summary>
    /// <param name="cacheType"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static Type GetOrCreate(CacheType cacheType, string tenantId)
    {
        //return _caches.GetOrAdd($"{cacheType}_{tenantId}", _ =>
        //{
        //    var type = GetOrCreateMark(tenantId);

        //    switch (cacheType)
        //    {
        //        case CacheType.MemoryCache:
        //            return typeof(MemoryCache<>).MakeGenericType(type);
        //        case CacheType.RedisCache:
        //            return typeof(RedisCache<>).MakeGenericType(type);
        //        case CacheType.SqlSugarCache:
        //            return typeof(SqlSugarCache<>).MakeGenericType(type);
        //        default:
        //            throw new NotImplementedException();
        //    }
        //});

        switch (cacheType)
        {
            case CacheType.MemoryCache:
                return GetOrCreate(typeof(MemoryCache<>), tenantId);
            case CacheType.RedisCache:
                return GetOrCreate(typeof(RedisCache<>), tenantId);
            case CacheType.SqlSugarCache:
                return GetOrCreate(typeof(SqlSugarCache<>), tenantId);
            default:
                throw new NotImplementedException();
        }

    }

    /// <summary>
    /// 获取缓存类
    /// </summary>
    /// <param name="cacheType"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static Type GetOrCreate(Type type, string tenantId)
    {
        //return _caches.GetOrAdd($"{cacheType}_{tenantId}", _ =>
        //{
        //    var type = GetOrCreateMark(tenantId);

        //    return cacheType.MakeGenericType(type);
        //});


        if (!type.IsGenericType)
        {
            throw new NotSupportedException(type.FullName);
        }
        if (type.GetGenericArguments().Length != 1)
        {
            throw new NotSupportedException($"[{type.FullName}]泛型参数只能有一个");
        }
        var key = $"{type.FullName}_{tenantId}";

        if (!_caches.TryGetValue(key, out Type target))
        {
            lock (_lock)
            {
                if (!_caches.TryGetValue(key, out target))
                {
                    target = _caches.GetOrAdd(key, _ =>
                    {
                        var mark = GetOrCreateMark(tenantId);

                        return type.MakeGenericType(mark);
                    });
                }
            }
        }
        return target;
    }

    /// <summary>
    /// 根据类型，获取租户号
    /// </summary>
    /// <returns></returns>
    public static string GetTenantId<T>() => typeof(T).Name.Replace(DynamicClassPrefix, "");
}
