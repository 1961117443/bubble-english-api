using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.TeamLog;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class TeamLogCrInput
{
    /// <summary>
    /// 团队id.
    /// </summary>
    public string? teamId { get; set; }


    /// <summary>
    /// 日志内容.
    /// </summary>
    public string? content { get; set; }

    /// <summary>
    /// 图片.
    /// </summary>
    public string? imageJson { get; set; }


    /// <summary>
    /// 附件.
    /// </summary>
    public string? attachJson { get; set; }
}
