using Org.BouncyCastle.Bcpg;
using QT.DependencyInjection;
using QT.Extend.Entitys.Dto.ProjectGantt;

namespace QT.Extend.Entitys.Dto.FinancialRecord;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class FinancialRecordListOutput: FinancialRecordInfoOutput
{
}

/// <summary>
/// 个人记账月份统计
/// </summary>
public class FinancialRecordMonthlyStatistics
{
    /// <summary>
    /// 分类 支出还是收入
    /// </summary>
    public int category { get; set; }

    /// <summary>
    /// 月份
    /// </summary>
    public string mon { get; set; }

    /// <summary>
    /// 金额汇总
    /// </summary>
    public decimal amount { get; set; }
}

public class EchartOutput
{
    public XAxis xAxis { get; set; }

    public List<SerieItem> series { get; set; }
}

public class XAxis
{
    public List<string> data { get; set; }
}

public class SerieItem
{
    public string name { get; set; }
    public string type { get; set; }


    public List<object> data { get; set; }
}