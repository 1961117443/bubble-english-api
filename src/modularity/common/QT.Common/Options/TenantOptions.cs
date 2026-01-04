using QT.ConfigurableOptions;

namespace QT.Common.Options;

/// <summary>
/// 租户配置.
/// </summary>
public sealed class TenantOptions : IConfigurableOptions
{
    /// <summary>
    /// 是否多租户模式.
    /// </summary>
    public bool MultiTenancy { get; set; }

    /// <summary>
    /// 多租户数据接口.
    /// </summary>
    public string MultiTenancyDBInterFace { get; set; }

    /// <summary>
    /// 多租户接口令牌.
    /// </summary>
    public string UserKey { get; set; }
    /// <summary>
    /// 多租户接口地址.
    /// </summary>
    public string MultiTenancyHost { get; set; }
}