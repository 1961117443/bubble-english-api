using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogArticle;

/// <summary>
/// 消息修改输入.
/// </summary>
[SuppressSniffer]
public class LogArticleUpInput : LogArticleCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}