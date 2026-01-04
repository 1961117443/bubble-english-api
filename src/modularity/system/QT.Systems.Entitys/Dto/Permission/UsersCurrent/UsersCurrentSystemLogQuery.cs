using System.Text.Json.Serialization;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.JsonSerialization;

namespace QT.Systems.Entitys.Dto.UsersCurrent;

/// <summary>
/// 当前用户系统日记查询输入.
/// </summary>
[SuppressSniffer]
public class UsersCurrentSystemLogQuery : PageInputBase
{
    /// <summary>
    /// 日记类型.
    /// </summary>
    public int category { get; set; }

    /// <summary>
    /// 开始时间.
    /// </summary>
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime? startTime { get; set; }

    /// <summary>
    /// 结束时间.
    /// </summary>
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime? endTime { get; set; }
}