using QT.DependencyInjection;

namespace QT.WorkFlow.Entitys.Model;

[SuppressSniffer]
public class FlowBeforeRecordListModel
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string? id { get; set; }

    /// <summary>
    /// 审批人.
    /// </summary>
    public string? handleId { get; set; }

    /// <summary>
    /// 审批时间.
    /// </summary>
    public DateTime? handleTime { get; set; }

    /// <summary>
    /// 审批意见.
    /// </summary>
    public string? handleOpinion { get; set; }

    /// <summary>
    /// 审批状态.
    /// </summary>
    public int? handleStatus { get; set; }

    /// <summary>
    /// 审批人名.
    /// </summary>
    public string? userName { get; set; }

    /// <summary>
    /// 分类id.
    /// </summary>
    public string? category { get; set; }

    /// <summary>
    /// 分类名.
    /// </summary>
    public string? categoryName { get; set; }

    /// <summary>
    /// 流转操作人.
    /// </summary>
    public string? operatorId { get; set; }
}
