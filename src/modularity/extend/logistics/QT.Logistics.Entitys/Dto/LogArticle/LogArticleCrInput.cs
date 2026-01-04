using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogArticle;

/// <summary>
/// 消息创建输入.
/// </summary>
[SuppressSniffer]
public class LogArticleCrInput
{
    /// <summary>
    /// 类别：1-园区介绍，2- 招商公告.
    /// </summary>
    public int? type { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// 正文内容.
    /// </summary>
    public string bodyText { get; set; }

    /// <summary>
    /// 收件用户.
    /// </summary>
    public string toUserIds { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    public string files { get; set; }

    /// <summary>
    /// 唯一标识.
    /// </summary>
    public string uniqueKey { get; set; }
}