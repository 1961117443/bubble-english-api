using QT.Common.Cache;
using QT.Common.Enum;
using QT.Common.Options;
using QT.DependencyInjection;

namespace QT.Common.Configuration;

/// <summary>
/// Key常量.
/// </summary>
[SuppressSniffer]
public class KeyVariable
{
    private static readonly TenantOptions _tenant = App.GetConfig<TenantOptions>("Tenant", true);

    private static readonly AppOptions _qt = App.GetConfig<AppOptions>("QT_App", true);

    private static readonly OssOptions Oss = App.GetConfig<OssOptions>("OSS", true);

    private static readonly CacheOptions _cacheOptions = App.GetConfig<CacheOptions>("Cache", true);

    private static readonly ConnectionStringsOptions _connectionString = App.GetConfig<ConnectionStringsOptions>("ConnectionStrings", true);
    /// <summary>
    /// 多租户模式.
    /// </summary>
    public static bool MultiTenancy
    {
        get
        {
            return _tenant.MultiTenancy;
        }
    }

    /// <summary>
    /// 多租户数据接口
    /// </summary>
    public static string MultiTenancyDBInterFace => _tenant.MultiTenancyDBInterFace;

    /// <summary>
    /// 默认的数据库编号
    /// </summary>
    public static string DefaultDbConfigId => _connectionString.ConfigId;

    /// <summary>
    /// 多租户数据接口地址
    /// </summary>
    public static string MultiTenancyHost => _tenant.MultiTenancyHost;


    /// <summary>
    /// 多租户数据接口令牌
    /// </summary>
    public static string MultiTenancyUserKey => _tenant.UserKey;

    /// <summary>
    /// 系统文件路径.
    /// </summary>
    public static string SystemPath
    {
        get
        {
            return Oss.Provider.Equals(OSSProviderType.Invalid) ? (string.IsNullOrEmpty(_qt.SystemPath) ? Directory.GetCurrentDirectory() : _qt.SystemPath) : string.Empty;
        }
    }

    /// <summary>
    /// 命名空间.
    /// </summary>
    public static List<string> AreasName
    {
        get
        {
            return string.IsNullOrEmpty(_qt.CodeAreasName.ToString()) ? new List<string>() : _qt.CodeAreasName;
        }
    }

    /// <summary>
    /// 允许上传图片类型.
    /// </summary>
    public static List<string> AllowImageType
    {
        get
        {
            return string.IsNullOrEmpty(_qt.AllowUploadImageType.ToString()) ? new List<string>() : _qt.AllowUploadImageType;
        }
    }

    /// <summary>
    /// 允许上传文件类型.
    /// </summary>
    public static List<string> AllowUploadFileType
    {
        get
        {
            return string.IsNullOrEmpty(_qt.AllowUploadFileType.ToString()) ? new List<string>() : _qt.AllowUploadFileType;
        }
    }

    /// <summary>
    /// 微信允许上传文件类型.
    /// </summary>
    public static List<string> WeChatUploadFileType
    {
        get
        {
            return string.IsNullOrEmpty(_qt.WeChatUploadFileType.ToString()) ? new List<string>() : _qt.WeChatUploadFileType;
        }
    }

    /// <summary>
    /// MinIO桶.
    /// </summary>
    public static string BucketName
    {
        get
        {
            return string.IsNullOrEmpty(Oss.BucketName) ? string.Empty : Oss.BucketName;
        }
    }

    /// <summary>
    /// 文件储存类型.
    /// </summary>
    public static OSSProviderType FileStoreType
    {
        get
        {
            return string.IsNullOrEmpty(Oss.Provider.ToString()) ? OSSProviderType.Invalid : Oss.Provider;
        }
    }

    /// <summary>
    /// App版本.
    /// </summary>
    public static string AppVersion
    {
        get
        {
            return string.IsNullOrEmpty(App.Configuration["QT_APP:AppVersion"]) ? string.Empty : App.Configuration["QT_APP:AppVersion"];
        }
    }

    /// <summary>
    /// 文件储存类型.
    /// </summary>
    public static string AppUpdateContent
    {
        get
        {
            return string.IsNullOrEmpty(App.Configuration["QT_APP:AppUpdateContent"]) ? string.Empty : App.Configuration["QT_APP:AppUpdateContent"];
        }
    }


    /// <summary>
    /// 系统的缓存类型
    /// </summary>
    public static CacheType CacheType => _cacheOptions?.CacheType ?? CacheType.MemoryCache;
}