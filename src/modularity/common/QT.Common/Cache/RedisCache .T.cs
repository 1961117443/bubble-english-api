using QT.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using CSRedis;
using QT.Common.Extension;
using Microsoft.AspNetCore.Components;

namespace QT.Common.Cache;

/// <summary>
/// Redis缓存泛型类（目前用于多租户）.
/// </summary>
public class RedisCache<T> : ICache,ISingleton
{
    //public CSRedisClient Client => RedisHelper<T>.Instance;

    private static string _tenantClassName = typeof(T).Name;

    private readonly CSRedisClient _client;

    private string _cachePrfix = string.Empty;
    /// <summary>
    /// 构造函数.
    /// </summary>
    public RedisCache(IOptions<CacheOptions> cacheOptions)
    {
        _client = new CSRedisClient(CreateConnection(cacheOptions.Value));
        RedisHelper<T>.Initialization(_client);
        //RedisHelper.Initialization(csredis);
        //RedisHelper<T>.Set("abcd", "234");
        //Console.WriteLine(RedisHelper<T>.Get("abcd"));
    }

    private string CreateConnection(CacheOptions cacheOptions)
    {
        var tenantId = typeof(T).Name.Replace(TenantCacheFactory.DynamicClassPrefix, "");
        // 判断是否有prefix前缀
        var arr = cacheOptions.RedisConnectionString.Split(",").ToList();
        //string prefix = string.Empty;
        bool exists = false;
        for (int i = 0; i < arr.Count; i++)
        {
            var item = arr[i];
            var str = item.ToLower().Replace(" ", "");
            if (str.StartsWith("prefix="))
            {
                str = str.Replace("prefix=", "");
                if (!string.IsNullOrEmpty(str))
                {
                    if (!str.EndsWith(":"))
                    {
                        str += ":";
                    }
                }
                arr[i] = $"prefix={str}tenant_{tenantId}:";
                _cachePrfix = $"{str}tenant_{tenantId}:";
                exists = true;
                break;
            }

        }
        if (!exists)
        {
            arr.Add($"prefix=tenant_{tenantId}:");
            _cachePrfix = $"tenant_{tenantId}:";
        }

        return string.Join(",", arr);
    }

    /// <summary>
    /// 获取真正的key，不含prefix前辍
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string GetRealKey(string key)
    {
        if (key.StartsWith(_cachePrfix))
        {
            return key.Substring(_cachePrfix.Length);
        }
        return key;
    }


    /// <summary>
    /// 用于在 key 存在时删除 key.
    /// </summary>
    /// <param name="key">键.</param>
    public long Del(params string[] key)
    {
        key = key.Select(x => GetRealKey(x)).ToArray();
        return _client.Del(key);
    }

    /// <summary>
    /// 用于在 key 存在时删除 key.
    /// </summary>
    /// <param name="key">键.</param>
    public Task<long> DelAsync(params string[] key)
    {
        key = key.Select(x => GetRealKey(x)).ToArray();
        return _client.DelAsync(key);
    }

    /// <summary>
    /// 用于在 key 模板存在时删除.
    /// </summary>
    /// <param name="pattern">key模板.</param>
    public async Task<long> DelByPatternAsync(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return default;

        // pattern = Regex.Replace(pattern, @"\{.*\}", "*");
        string[]? keys = await _client.KeysAsync(pattern);
        if (keys?.Length > 0)
        {
            keys = keys.Select(x => GetRealKey(x)).ToArray();
            return await _client.DelAsync(keys);
        }

        return default;
    }

    /// <summary>
    /// 检查给定 key 是否存在.
    /// </summary>
    /// <param name="key">键.</param>
    public bool Exists(string key)
    {
        return _client.Exists(GetRealKey(key));
    }

    /// <summary>
    /// 检查给定 key 是否存在.
    /// </summary>
    /// <param name="key">键.</param>
    public Task<bool> ExistsAsync(string key)
    {
        return _client.ExistsAsync(GetRealKey(key));
    }

    /// <summary>
    /// 获取指定 key 的增量值.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="incrBy">增量.</param>
    /// <returns></returns>
    public long Incrby(string key, long incrBy)
    {
        return _client.IncrBy(GetRealKey(key), incrBy);
    }

    /// <summary>
    /// 获取指定 key 的增量值.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="incrBy">增量.</param>
    /// <returns></returns>
    public Task<long> IncrbyAsync(string key, long incrBy)
    {
        return _client.IncrByAsync(GetRealKey(key), incrBy);
    }

    /// <summary>
    /// 获取指定 key 的值.
    /// </summary>
    /// <param name="key">键.</param>
    public string Get(string key)
    {
        return _client.Get(GetRealKey(key));
    }

    /// <summary>
    /// 获取指定 key 的值.
    /// </summary>
    /// <typeparam name="T">byte[] 或其他类型.</typeparam>
    /// <param name="key">键.</param>
    public T Get<T>(string key)
    {
        return _client.Get<T>(GetRealKey(key));
    }

    /// <summary>
    /// 获取指定 key 的值.
    /// </summary>
    /// <param name="key">键.</param>
    /// <returns></returns>
    public Task<string> GetAsync(string key)
    {
        return _client.GetAsync(GetRealKey(key));
    }

    /// <summary>
    /// 获取指定 key 的值.
    /// </summary>
    /// <typeparam name="T">byte[] 或其他类型.</typeparam>
    /// <param name="key">键.</param>
    public Task<T> GetAsync<T>(string key)
    {
        return _client.GetAsync<T>(GetRealKey(key));
    }

    /// <summary>
    /// 设置指定 key 的值，所有写入参数object都支持string | byte[] | 数值 | 对象.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    public bool Set(string key, object value)
    {
        return _client.Set(GetRealKey(key), value);
    }

    /// <summary>
    /// 设置指定 key 的值，所有写入参数object都支持string | byte[] | 数值 | 对象.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    /// <param name="expire">有效期.</param>
    public bool Set(string key, object value, TimeSpan expire)
    {
        return _client.Set(GetRealKey(key), value, expire);
    }

    /// <summary>
    /// 设置指定 key 的值，所有写入参数object都支持string | byte[] | 数值 | 对象.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    public Task<bool> SetAsync(string key, object value)
    {
        return _client.SetAsync(GetRealKey(key), value);
    }

    /// <summary>
    /// 保存.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    /// <param name="expire">过期时间.</param>
    /// <returns></returns>
    public Task<bool> SetAsync(string key, object value, TimeSpan expire)
    {
        return _client.SetAsync(GetRealKey(key), value, expire);
    }

    /// <summary>
    /// 只有在 key 不存在时设置 key 的值.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    /// <param name="expire">有效期.</param>
    public bool SetNx(string key, object value, TimeSpan expire)
    {
        if (_client.SetNx(GetRealKey(key), value))
        {
            _client.Set(GetRealKey(key), value, expire);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 只有在 key 不存在时设置 key 的值.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    public bool SetNx(string key, object value)
    {
       return _client.SetNx(GetRealKey(key), value);
    }

    /// <summary>
    /// 获取所有key.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetAllKeys()
    {
        foreach (var item in _client.Keys($"{_cachePrfix}*"))
        {
            yield return item;
        }
        //return _client.Keys($"{_tenantClassName}*").ToList();
    }

    /// <summary>
    /// 获取缓存过期时间.
    /// </summary>
    /// <param name="key">键值.</param>
    /// <returns></returns>
    public DateTime GetCacheOutTime(string key)
    {
        long second = _client.PTtl(GetRealKey(key));
        return DateTime.Now.AddMilliseconds(second);
    }
}