using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.SysLog;

/// <summary>
/// 操作日记输出.
/// </summary>
[SuppressSniffer]
public class LogOperationOutput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 请求时间.
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 请求用户名.
    /// </summary>
    public string userName { get; set; }

    /// <summary>
    /// 请求IP.
    /// </summary>
    public string ipaddress { get; set; }

    /// <summary>
    /// 请求设备.
    /// </summary>
    public string platForm { get; set; }

    /// <summary>
    /// 操作模块.
    /// </summary>
    public string moduleName { get; set; }

    /// <summary>
    /// 操作类型.
    /// </summary>
    public string requestMethod { get; set; }

    /// <summary>
    /// 请求耗时.
    /// </summary>
    public int requestDuration { get; set; }

    /// <summary>
    /// 操作记录.
    /// </summary>
    public string json { get; set; }
}