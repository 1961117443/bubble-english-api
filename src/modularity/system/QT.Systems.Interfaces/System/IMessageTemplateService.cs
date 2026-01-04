using QT.Systems.Entitys.System;

namespace QT.Systems.Interfaces.System;

/// <summary>
/// 业务契约：消息模板.
/// </summary>
public interface IMessageTemplateService
{
    /// <summary>
    /// 获取信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    Task<MessageTemplateEntity> GetInfo(string id);

    /// <summary>
    /// 发送通知.
    /// </summary>
    /// <param name="typeList">推送方式</param>
    /// <param name="messageTemplateEntity">标题</param>
    /// <param name="userList">接收用户</param>
    /// <param name="parameters"></param>
    /// <param name="bodyDic"></param>
    /// <returns></returns>
    Task SendNodeMessage(List<string> typeList, MessageTemplateEntity messageTemplateEntity, List<string> userList, Dictionary<string, string> parameters, Dictionary<string, object> bodyDic);
}