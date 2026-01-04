using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.WorkFlow.Entitys.Model;

[SuppressSniffer]
public class FlowHandleModel : PageInputBase
{
    /// <summary>
    /// 意见.
    /// </summary>
    public string? handleOpinion { get; set; }

    /// <summary>
    /// 加签人.
    /// </summary>
    public string? freeApproverUserId { get; set; }

    /// <summary>
    /// 自定义抄送人.
    /// </summary>
    public string? copyIds { get; set; }

    /// <summary>
    /// 流程编码.
    /// </summary>
    public string? enCode { get; set; }

    /// <summary>
    /// 表单数据.
    /// </summary>
    public object? formData { get; set; }

    /// <summary>
    /// 流程监控指派节点.
    /// </summary>
    public string? nodeCode { get; set; }

    /// <summary>
    /// 电子签名.
    /// </summary>
    public string? signImg { get; set; }

    /// <summary>
    /// 候选人.
    /// </summary>
    public Dictionary<string, List<string>>? candidateList { get; set; }

    /// <summary>
    /// 批量id.
    /// </summary>
    public List<string> ids { get; set; } = new List<string>();

    /// <summary>
    /// 批量类型.
    /// </summary>
    public int batchType { get; set; }
}
