using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.QuoteOrderLog;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class QuoteOrderLogCrInput
{
    /// <summary>
    /// 报价单id.
    /// </summary>
    public string? fid { get; set; }


    /// <summary>
    /// 问题描述.
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
