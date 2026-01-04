using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.QuoteOrderShare;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class QuoteOrderShareCrInput
{
    /// <summary>
    /// 报价单id.
    /// </summary>
    public string? fid { get; set; }


    /// <summary>
    /// 查看密码.
    /// </summary>
    public string? password { get; set; }

    /// <summary>
    /// 最多查看次数.
    /// </summary>
    public int? maxViewCount { get; set; }


    /// <summary>
    /// 过期时间.
    /// </summary>
    public DateTime? expiryTime { get; set; }
}
