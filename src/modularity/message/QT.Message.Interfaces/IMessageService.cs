namespace QT.Message.Interfaces.Message;

/// <summary>
/// 系统消息



/// 日 期：2021-06-01.
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// 消息发送.
    /// </summary>
    /// <param name="toUserIds"></param>
    /// <param name="title"></param>
    /// <param name="bodyText"></param>
    /// <returns></returns>
    Task SentMessage(List<string> toUserIds, string title, string bodyText = null, Dictionary<string, object> bodyDic = null);
}