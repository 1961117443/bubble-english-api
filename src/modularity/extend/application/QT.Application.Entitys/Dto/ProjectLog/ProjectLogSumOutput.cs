using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.ProjectLog;

/// <summary>
/// 日志数量统计（按天）
/// </summary>
[SuppressSniffer]
public class ProjectLogSumOutput
{
    /// <summary>
    /// 日期
    /// </summary>
    public string date { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int count { get; set; }

    /// <summary>
    /// 服务中
    /// </summary>
    public int enable { get; set; }
}