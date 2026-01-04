using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using QT.DependencyInjection;

namespace QT.Common.Cache;

/// <summary>
/// 内存缓存泛型类（目前用于多租户）
/// </summary>
/// <typeparam name="T">租户类型</typeparam>
public class MemoryCache<T> : MemoryCache, ICache, ISingleton
{
    /// <summary>
    /// 初始化一个<see cref="MemoryCache"/>类型的新实例
    /// </summary>
    /// <param name="optionsAccessor"></param>
    public MemoryCache(IOptions<MemoryCacheOptions> optionsAccessor):base(new Microsoft.Extensions.Caching.Memory.MemoryCache(optionsAccessor))
    {
        //_memoryCache = new Microsoft.Extensions.Caching.Memory.MemoryCache(optionsAccessor);
    }
}