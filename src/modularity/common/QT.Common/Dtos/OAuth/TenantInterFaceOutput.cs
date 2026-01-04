using QT.Common.Enum;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.Extras.DatabaseAccessor.SqlSugar;

namespace QT.Common.Dtos.OAuth;

/// <summary>
/// 多租户网络连接输出.
/// </summary>
[SuppressSniffer]
public class TenantInterFaceOutput
{
    /// <summary>
    /// DotNet.
    /// </summary>
    public string? dotnet { get; set; }

    /// <summary>
    /// 配置连接.
    /// </summary>
    public List<TenantLinkModel>? linkList { get; set; }

    /// <summary>
    /// 租户oss配置
    /// </summary>
    public TenantOssConfig ossConfig { get; set; }
}

/// <summary>
/// 多租户网络连接输出.
/// </summary>
[SuppressSniffer]
public class TenantListInterFaceOutput : TenantInterFaceOutput, ITenantDbInfo
{
    /// <summary>
    /// 开始时间(带毫秒的时间戳)
    /// </summary>
    public long startTime { get; set; }

    /// <summary>
    /// 结束时间(带毫秒的时间戳)
    /// </summary>
    public long endTime { get; set; }

    /// <summary>
    /// 租户编号
    /// </summary>
    public string enCode { get; set; }


    /// <inheritdoc/>
    public bool enable
    {
        get
        {
            var current = DateTime.Now.ParseToUnixTime();

            return current >= startTime && current <= endTime;
        }
    }
}



/// <summary>
/// 租户OSS配置
/// </summary>
public class TenantOssConfig
{
    /// <summary>
    /// 存储桶名称
    /// </summary>
    public string BucketName { get; set; }

    /// <summary>
    /// 文件存储类型(Invalid(本地),MinIo,Aliyun,QCloud,Qiniu)
    /// </summary>
    public OSSProviderType Provider { get; set; } = OSSProviderType.Invalid;

    /// <summary>
    /// 端点地址
    /// </summary>
    public string Endpoint { get; set; }

    /// <summary>
    /// 访问密钥和密钥
    /// </summary>
    public string AccessKey { get; set; }

    /// <summary>
    /// 密钥
    /// </summary>
    public string SecretKey { get; set; }

    /// <summary>
    /// 是否启用HTTPS
    /// </summary>
    public bool IsEnableHttps { get; set; }

    /// <summary>
    /// 是否启用缓存
    /// </summary>
    public bool IsEnableCache { get; set; }


    private string _region = "us-east-1";
    /// <summary>
    /// 区域信息（如阿里云的oss区域）
    /// </summary>
    public string Region
    {
        get
        {
            return _region;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                _region = "us-east-1";
            }
            else
            {
                _region = value;
            }
        }
    }
}