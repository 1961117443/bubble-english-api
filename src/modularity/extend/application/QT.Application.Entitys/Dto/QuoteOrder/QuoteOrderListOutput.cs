using QT.DependencyInjection;
using QT.Extend.Entitys.Dto.ProjectGantt;

namespace QT.Extend.Entitys.Dto.QuoteOrder;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class QuoteOrderListOutput: QuoteOrderInfoOutput
{ 
    /// <summary>
    /// 客户名称
    /// </summary>
    public string cidName { get; set; }

    /// <summary>
    /// 客户电话
    /// </summary>
    public string cidAdmintel { get;set; }

    /// <summary>
    /// 浏览次数
    /// </summary>
    public int? viewCount { get; set; }

    /// <summary>
    /// 最后浏览时间
    /// </summary>
    public DateTime? lastViewTime { get; set; }
}
