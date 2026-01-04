using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.MessageTemplate;

/// <summary>
/// base_message_template更新输入参数.
/// </summary>
[SuppressSniffer]
public class MessageTemplateUpInput : MessageTemplateCrInput
{
    /// <summary>
    /// 自然主键.
    /// </summary>
    public string id { get; set; }
}