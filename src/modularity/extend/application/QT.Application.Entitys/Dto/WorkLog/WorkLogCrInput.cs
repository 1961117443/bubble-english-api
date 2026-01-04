using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.WorkLog;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class WorkLogCrInput
{
    /// <summary>
    /// 标题.
    /// </summary>
    public string? title { get; set; }

    /// <summary>
    /// 问题内容.
    /// </summary>
    public string? question { get; set; }

    /// <summary>
    /// 今日内容.
    /// </summary>
    public string? todayContent { get; set; }

    /// <summary>
    /// 明日内容.
    /// </summary>
    public string? tomorrowContent { get; set; }

    /// <summary>
    /// 接收人.
    /// </summary>
    public string? toUserId { get; set; }

    /// <summary>
    /// 用户id.
    /// </summary>
    public string? userIds { get; set; }


    public int? enabledMark { get; set; }


    /// <summary>
    /// 图片.
    /// </summary>
    public string? imageJson { get; set; }


    /// <summary>
    /// 附件.
    /// </summary>
    public string? attachJson { get; set; }

    /// <summary>
    /// 日志类型
    /// </summary>
    public int? category { get; set; }
}
