using QT.Common.Filter;
using System.ComponentModel.DataAnnotations;

namespace QT.Extend.Entitys.Dto.ProjectLog;

public class ProjectLogListPageInput: PageInputBase
{
    /// <summary>
    /// 团队id
    /// </summary>
    [Required]
    public string projectId { get; set; }

    /// <summary>
    /// 日志日期
    /// </summary>
    public string date { get; set; }
}