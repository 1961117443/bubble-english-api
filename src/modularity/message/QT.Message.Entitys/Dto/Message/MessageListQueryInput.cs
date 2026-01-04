using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.Message.Entitys.Dto.Message;

/// <summary>
/// 消息列表查询输入.
/// </summary>
[SuppressSniffer]
public class MessageListQueryInput : PageInputBase
{
    /// <summary>
    /// 类型.
    /// </summary>
    public int? type { get; set; }
}