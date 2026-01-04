using QT.ConfigurableOptions;

namespace QT.Common.Options;

/// <summary>
/// 数据库配置.
/// </summary>
public sealed class ConnectionStringsOptions : IConfigurableOptions
{
    /// <summary>
    /// 默认数据库编号.
    /// </summary>
    public string ConfigId { get; set; }

    /// <summary>
    /// 数据库类型.
    /// </summary>
    public string DBType { get; set; }

    /// <summary>
    /// 数据库名称.
    /// </summary>
    public string DBName { get; set; }

    /// <summary>
    /// 数据库地址.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// 数据库端口号.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 账号.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 密码.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 默认数据库连接字符串.
    /// 2024.3.6 sxn 多租户估计不需要
    /// </summary>
    public string DefaultConnection { get; set; }
}