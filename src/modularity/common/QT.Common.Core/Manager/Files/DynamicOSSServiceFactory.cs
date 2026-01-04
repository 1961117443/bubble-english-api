using OnceMi.AspNetCore.OSS;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using QT.Common.Core.Manager.Tenant;
using Microsoft.Extensions.Caching.Memory;
using QT.Extras.DatabaseAccessor.SqlSugar;
using QT.Common.Dtos.OAuth;
using Mapster;

namespace QT.Common.Core.Manager.Files;

/// <summary>
/// 
/// </summary>
public interface IDynamicOSSServiceFactory
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    IOSSService Create(TenantOssConfig ossConfig);
}
/// <summary>
/// 动态构建OSSService
/// </summary>
public class DynamicOSSServiceFactory: IDynamicOSSServiceFactory
{
    private readonly IOptionsMonitor<OSSOptions> optionsMonitor;

    private readonly ICacheProvider _cache;

    private readonly ILoggerFactory _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public DynamicOSSServiceFactory(ICacheProvider provider, ILoggerFactory logger)
    {
        //this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException();
        this._cache = provider ?? throw new ArgumentNullException("IMemoryCache");
        this._logger = logger ?? throw new ArgumentNullException("ILoggerFactory");
    }
    public IOSSService Create(TenantOssConfig ossConfig)
    {
        var oSSOptions = ossConfig?.Adapt<OSSOptions>();
        if (oSSOptions == null || (oSSOptions.Provider == OSSProvider.Invalid && string.IsNullOrEmpty(oSSOptions.Endpoint) && string.IsNullOrEmpty(oSSOptions.SecretKey) && string.IsNullOrEmpty(oSSOptions.AccessKey)))
        {
            throw new ArgumentException("Cannot get option by tenant.");
        }

        if (oSSOptions.Provider == OSSProvider.Invalid)
        {
            throw new ArgumentNullException("Provider");
        }

        if (string.IsNullOrEmpty(oSSOptions.Endpoint) && oSSOptions.Provider != OSSProvider.Qiniu)
        {
            throw new ArgumentNullException("Endpoint", "When your provider is Minio/QCloud/Aliyun/HuaweiCloud, endpoint can not null.");
        }

        if (string.IsNullOrEmpty(oSSOptions.SecretKey))
        {
            throw new ArgumentNullException("SecretKey", "SecretKey can not null.");
        }

        if (string.IsNullOrEmpty(oSSOptions.AccessKey))
        {
            throw new ArgumentNullException("AccessKey", "AccessKey can not null.");
        }

        if ((oSSOptions.Provider == OSSProvider.Minio || oSSOptions.Provider == OSSProvider.QCloud || oSSOptions.Provider == OSSProvider.Qiniu || oSSOptions.Provider == OSSProvider.HuaweiCloud) && string.IsNullOrEmpty(oSSOptions.Region))
        {
            throw new ArgumentNullException("Region", "When your provider is Minio/QCloud/Qiniu/HuaweiCloud, region can not null.");
        }

        return oSSOptions.Provider switch
        {
            OSSProvider.Aliyun => new AliyunOSSService(_cache, oSSOptions),
            OSSProvider.Minio => new MinioOSSService(_cache, oSSOptions),
            OSSProvider.QCloud => new QCloudOSSService(_cache, oSSOptions),
            OSSProvider.Qiniu => new QiniuOSSService(_cache, oSSOptions),
            OSSProvider.HuaweiCloud => new HaweiOSSService(_cache, oSSOptions),
            //OSSProvider.BaiduCloud => new BaiduOSSService(_cache, oSSOptions),
            //OSSProvider.Ctyun => new CtyunOSSService(_cache, oSSOptions),
            _ => throw new Exception("Unknow provider type"),
        };
    }
}
