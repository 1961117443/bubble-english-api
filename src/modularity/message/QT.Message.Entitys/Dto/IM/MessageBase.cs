using System.Text.Json.Serialization;
using QT.DependencyInjection;
using QT.Message.Entitys.Enums;

namespace QT.Message.Entitys.Dto.IM;

/// <summary>
/// 消息基类类.
/// </summary>
[SuppressSniffer]
public class MessageBase
{
    /// <summary>
    /// 方法.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageSendType method { get; set; }
}