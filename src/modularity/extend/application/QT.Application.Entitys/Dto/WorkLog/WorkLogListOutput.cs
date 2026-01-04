using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.WorkLog;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class WorkLogListOutput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    public string? title { get; set; }

    /// <summary>
    /// 问题内容.
    /// </summary>
    public string? question { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 今日内容.
    /// </summary>
    public string? todayContent { get; set; }

    /// <summary>
    /// 明日内容.
    /// </summary>
    public string? tomorrowContent { get; set; }

    /// <summary>
    /// 接收人.
    /// </summary>
    public string? toUserId { get; set; }

    /// <summary>
    /// 修改时间.
    /// </summary>
    public DateTime? lastModifyTime { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    public long? sortCode { get; set; }


    /// <summary>
    /// 日志类型.
    /// </summary>
    public int? category { get; set; }

    /// <summary>
    /// 图片.
    /// </summary>
    public string? imageJson { get; set; }


    /// <summary>
    /// 附件.
    /// </summary>
    public string? attachJson { get; set; }

    /// <summary>
    /// 发件人.
    /// </summary>
    public string? sender { get; set; }
}

public class WorkLogListPageInput: PageInputBase
{
    /// <summary>
    /// 状态类型（draft | sent）
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public long? startTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public long? endTime { get; set; }

    /// <summary>
    /// 日志日期
    /// </summary>
    public string date { get; set; }
}

public class WorkLogSumOutput : TreeModel
{
    public int count { get; set; }

    public string mon { get; set; }

    public string year { get; set; }
}

public class WorkLogPersonalOutput
{
    /// <summary>
    /// id
    /// </summary>
    public string id { get; set; }



    /// <summary>
    /// 问题内容.
    /// </summary>
    public string? content { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    public string type { get; set; }
}