using QT.DependencyInjection;

namespace QT.WorkFlow.Entitys.Dto.FlowTask;

[SuppressSniffer]
public class FlowTaskCrInput
{
    /// <summary>
    /// 引擎id.
    /// </summary>
    public string? flowId { get; set; }

    /// <summary>
    /// 主键id.
    /// </summary>
    public string? id { get; set; }

    /// <summary>
    /// 界面数据.
    /// </summary>
    public string? data { get; set; }

    /// <summary>
    /// 提交/保存 0-1.
    /// </summary>
    public int? status { get; set; }

    /// <summary>
    /// 候选人.
    /// </summary>
    public Dictionary<string, List<string>>? candidateList { get; set; }
}
