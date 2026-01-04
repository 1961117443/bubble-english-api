using QT.DependencyInjection;
using QT.WorkFlow.Entitys.Model;

namespace QT.WorkFlow.Entitys.Dto.FlowBefore;

[SuppressSniffer]
public class FlowBeforeRecordListOutput
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string? fullName { get; set; }

    /// <summary>
    /// 经办记录.
    /// </summary>
    public List<FlowBeforeRecordListModel> list { get; set; }
}