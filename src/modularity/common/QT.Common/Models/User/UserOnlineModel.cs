using QT.DependencyInjection;

namespace QT.Common.Models.User;

/// <summary>
/// 在线用户模型.
/// </summary>
[SuppressSniffer]
public class UserOnlineModel
{
    /// <summary>
    /// 连接ID.
    /// </summary>
    public string connectionId { get; set; }

    /// <summary>
    /// 用户ID.
    /// </summary>
    public string userId { get; set; }

    /// <summary>
    /// 最后连接时间.
    /// </summary>
    public DateTime lastTime { get; set; }

    /// <summary>
    /// 最后登录IP.
    /// </summary>
    public string lastLoginIp { get; set; }

    /// <summary>
    /// 登录平台设备.
    /// </summary>
    public string lastLoginPlatForm { get; set; }

    /// <summary>
    /// 账号.
    /// </summary>
    public string account { get; set; }

    /// <summary>
    /// 用户名称.
    /// </summary>
    public string userName { get; set; }

    /// <summary>
    /// 租户id.
    /// </summary>
    public string tenantId { get; set; }

    /// <summary>
    /// token.
    /// </summary>
    public string token { get; set; }

    /// <summary>
    /// 是否移动端.
    /// </summary>
    public bool isMobileDevice { get; set; }
}