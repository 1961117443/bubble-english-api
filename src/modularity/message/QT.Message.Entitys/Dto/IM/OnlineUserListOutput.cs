using QT.DependencyInjection;

namespace QT.Message.Entitys.Dto.IM;

/// <summary>
/// 在线用户
/// </summary>
[SuppressSniffer]
public class OnlineUserListOutput
{
    /// <summary>
    /// 用户ID.
    /// </summary>
    public string userId { get; set; }

    /// <summary>
    /// 用户账号.
    /// </summary>
    public string userAccount { get; set; }

    /// <summary>
    /// 用户名称.
    /// </summary>
    public string userName { get; set; }

    /// <summary>
    /// 登录时间.
    /// </summary>
    public string loginTime { get; set; }

    /// <summary>
    /// 登录IP地址.
    /// </summary>
    public string loginIPAddress { get; set; }

    /// <summary>
    /// 登录平台设备.
    /// </summary>
    public string loginPlatForm { get; set; }

    /// <summary>
    /// 租户ID.
    /// </summary>
    public string tenantId { get; set; }
}