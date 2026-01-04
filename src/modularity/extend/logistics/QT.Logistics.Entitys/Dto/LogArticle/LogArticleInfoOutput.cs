using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogArticle;

/// <summary>
/// 消息信息输出.
/// </summary>
[SuppressSniffer]
public class LogArticleInfoOutput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// 正文内容.
    /// </summary>
    public string bodyText { get; set; }

    /// <summary>
    /// 发送人员.
    /// </summary>
    public string creatorUser { get; set; }

    /// <summary>
    /// 发送时间.
    /// </summary>
    public DateTime? lastModifyTime { get; set; }

    /// <summary>
    /// 收件用户.
    /// </summary>
    public string toUserIds { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    public string files { get; set; }


    /// <summary>
    /// 阅读次数.
    /// </summary>
    public int readCount { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    public int? type { get; set; }

    /// <summary>
    /// 唯一标识.
    /// </summary>
    public string uniqueKey { get; set; }
}