using QT.Systems.Entitys.Dto.SysConfig;

namespace QT.Systems.Interfaces.System;

/// <summary>
/// 业务契约：短信模板.
/// </summary>
public interface ISmsTemplateService
{
    /// <summary>
    /// 获取短信模板字段.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<List<string>> GetSmsTemplateFields(string id);

    /// <summary>
    /// 工作流发送短信.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sysconfig"></param>
    /// <param name="phoneNumbers"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    Task FlowTaskSend(string id, SysConfigOutput sysconfig, List<string> phoneNumbers, Dictionary<string, string> parameters);
}