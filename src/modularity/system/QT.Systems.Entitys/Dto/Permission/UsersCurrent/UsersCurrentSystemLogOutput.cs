using QT.DependencyInjection;
using System.Text.Json.Serialization;

namespace QT.Systems.Entitys.Dto.UsersCurrent;

/// <summary>
/// 当前用户系统日记输出.
/// </summary>
[SuppressSniffer]
public class UsersCurrentSystemLogOutput
{
    /// <summary>
    /// 登录时间.
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 登录用户.
    /// </summary>
    public string userName { get; set; }

    /// <summary>
    /// 登录IP.
    /// </summary>
    public string ipaddress { get; set; }

    /// <summary>
    /// 登录摘要.
    /// </summary>
    public string platForm { get; set; }

    /// <summary>
    /// 请求地址.
    /// </summary>
    public string requestURL { get; set; }

    /// <summary>
    /// 请求类型.
    /// </summary>
    public string requestMethod { get; set; }

    /// <summary>
    /// 请求耗时.
    /// </summary>
    public int? requestDuration { get; set; }

    /// <summary>
    /// 模块名称.
    /// </summary>
    [JsonIgnore]
    public string moduleName { get; set; }

    /// <summary>
    /// 用户ID.
    /// </summary>
    [JsonIgnore]
    public string userId { get; set; }

    /// <summary>
    /// 类型.
    /// </summary>
    [JsonIgnore]
    public int? category { get; set; }
}