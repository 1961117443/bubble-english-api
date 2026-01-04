using QT.DependencyInjection;

namespace QT.WorkFlow.Entitys.Model.Item;

[SuppressSniffer]
public class AssignItem
{
    /// <summary>
    /// 父流程字段.
    /// </summary>
    public string? parentField { get; set; }

    /// <summary>
    /// 子流程字段.
    /// </summary>
    public string? childField { get; set; }
}
