using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogArticle;

/// <summary>
/// 消息列表查询输入.
/// </summary>
[SuppressSniffer]
public class LogArticleListQueryInput : PageInputBase
{
    /// <summary>
    /// 类型.
    /// </summary>
    public int? type { get; set; }
}