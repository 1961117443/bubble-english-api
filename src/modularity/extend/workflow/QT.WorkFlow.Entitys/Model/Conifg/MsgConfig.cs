using QT.DependencyInjection;
using QT.WorkFlow.Entitys.Model.Item;

namespace QT.WorkFlow.Entitys.Model.Conifg;

[SuppressSniffer]
public class MsgConfig
{
    /// <summary>
    /// 关闭 0  自定义 1 同步发起配置 2.
    /// </summary>
    public int on { get; set; }

    /// <summary>
    /// 消息id.
    /// </summary>
    public string? msgId { get; set; }

    /// <summary>
    /// 消息名称.
    /// </summary>
    public string? msgName { get; set; }

    /// <summary>
    /// 模板配置json.
    /// </summary>
    public List<TemplateJsonItem>? templateJson { get; set; }
}
