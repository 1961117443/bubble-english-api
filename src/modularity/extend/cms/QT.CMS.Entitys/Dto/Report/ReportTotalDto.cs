using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Report;

/// <summary>
/// 统计数量DTO
/// </summary>
public class ReportTotalDto
{
    /// <summary>
    /// 显示标题
    /// </summary>
    [Display(Name = "显示标题")]
    public string? title { set; get; }

    /// <summary>
    /// 统计数量
    /// </summary>
    [Display(Name = "总数量")]
    public int total { set; get; }

    /// <summary>
    /// 总金额
    /// </summary>
    public decimal amount { set; get; }
}