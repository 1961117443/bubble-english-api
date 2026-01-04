using QT.DependencyInjection;

namespace QT.Message.Entitys.Dto.Message;

/// <summary>
/// 消息创建输入.
/// </summary>
[SuppressSniffer]
public class MessageCrInput
{
    /// <summary>
    /// 标题.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// 正文内容.
    /// </summary>
    public string bodyText { get; set; }

    /// <summary>
    /// 收件用户.
    /// </summary>
    public string toUserIds { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    public string files { get; set; }
}