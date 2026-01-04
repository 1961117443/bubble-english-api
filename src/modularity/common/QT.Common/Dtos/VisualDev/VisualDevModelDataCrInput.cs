using System.Collections.Generic;

namespace QT.Common.Dtos.VisualDev;

/// <summary>
/// 在线功能开发数据创建输入.
/// </summary>
public class VisualDevModelDataCrInput
{
    /// <summary>
    /// 数据.
    /// </summary>
    public string data { get; set; }

    /// <summary>
    /// 1-保存.
    /// </summary>
    public int status { get; set; }

    /// <summary>
    /// 候选人.
    /// </summary>
    public Dictionary<string, List<string>> candidateList { get; set; }

}
