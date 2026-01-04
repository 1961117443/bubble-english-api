using QT.Common.Filter;
using System.ComponentModel.DataAnnotations;

namespace QT.Extend.Entitys.Dto.TeamLog;

public class TeamLogListPageInput: PageInputBase
{
    /// <summary>
    /// 团队id
    /// </summary>
    [Required]
    public string teamId { get; set; }

    /// <summary>
    /// 日志日期
    /// </summary>
    public string date { get; set; }
}