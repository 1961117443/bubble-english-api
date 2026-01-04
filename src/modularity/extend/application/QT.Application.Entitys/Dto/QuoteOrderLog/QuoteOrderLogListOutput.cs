using QT.DependencyInjection;
using QT.Extend.Entitys.Dto.ProjectGantt;

namespace QT.Extend.Entitys.Dto.QuoteOrderLog;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class QuoteOrderLogListOutput: QuoteOrderLogInfoOutput
{
    public DateTime? creatorTime { get; set; }
    public string creatorUserId { get; set; }

    public ManagersInfo managerInfo { get; set; }
}
