using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrderLog;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class LogEnterpriseQuoteOrderLogListOutput: LogEnterpriseQuoteOrderLogInfoOutput
{
    public DateTime? creatorTime { get; set; }
    public string creatorUserId { get; set; }

    public ManagersInfo managerInfo { get; set; }
}


/// <summary>
/// 参加人员信息.
/// </summary>
public class ManagersInfo
{
    /// <summary>
    /// 账号+名字.
    /// </summary>
    public string? account { get; set; }

    /// <summary>
    /// 用户头像.
    /// </summary>
    public string? headIcon { get; set; }

    /// <summary>
    /// 名字第一个字
    /// </summary>
    public string firstName { get; set; }
}
