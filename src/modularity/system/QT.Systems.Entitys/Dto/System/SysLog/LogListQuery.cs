using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.SysLog;

/// <summary>
/// 系统日志列表入参.
/// </summary>
[SuppressSniffer]
public class LogListQuery : PageInputBase
{
    /// <summary>
    /// 开始时间.
    /// </summary>
    public long? startTime { get; set; }

    /// <summary>
    /// 结束时间.
    /// </summary>
    public long? endTime { get; set; }

    /// <summary>
    /// IP地址.
    /// </summary>
    public string ipaddress { get; set; }

    /// <summary>
    /// 用户.
    /// </summary>
    public string userName { get; set; }

    /// <summary>
    /// 请求方式.
    /// </summary>
    public string requestMethod { get; set; }

    /// <summary>
    /// 模块名.
    /// </summary>
    public string moduleName { get; set; }

    /// <summary>
    /// 日志key
    /// </summary>
    public string logKey { get; set; }
}