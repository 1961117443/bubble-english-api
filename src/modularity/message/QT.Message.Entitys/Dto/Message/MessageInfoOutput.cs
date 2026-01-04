using QT.DependencyInjection;

namespace QT.Message.Entitys.Dto.Message;

/// <summary>
/// 消息信息输出.
/// </summary>
[SuppressSniffer]
public class MessageInfoOutput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// 正文内容.
    /// </summary>
    public string bodyText { get; set; }

    /// <summary>
    /// 发送人员.
    /// </summary>
    public string creatorUser { get; set; }

    /// <summary>
    /// 发送时间.
    /// </summary>
    public DateTime? lastModifyTime { get; set; }

    /// <summary>
    /// 收件用户.
    /// </summary>
    public string toUserIds { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    public string files { get; set; }
}