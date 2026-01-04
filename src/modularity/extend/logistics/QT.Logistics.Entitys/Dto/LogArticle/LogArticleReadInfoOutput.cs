using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogArticle;

/// <summary>
/// 读取消息信息输出.
/// </summary>
[SuppressSniffer]
public class LogArticleReadInfoOutput
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
    /// 附件.
    /// </summary>
    public string files { get; set; }
}